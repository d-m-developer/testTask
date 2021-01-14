using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestTask
{
    class ListNode
    {
        public ListNode Prev;
        public ListNode Rand;
        public ListNode Next;
        public string Data;

        public ListNode(string data)
        {
            Data = data;
        }
    }
    class ListRand
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        /// <summary>
        /// Добавление элемента
        /// </summary>
        /// <param name="data">Данные</param>
        /// <returns>Ссылка на элемент</returns>
        public ListNode Add(string data)
        {
            var newNode = new ListNode(data);
            if (Head == null)
            {
                Head = newNode;
            }
            else
            {
                Tail.Next = newNode;
                newNode.Prev = Tail;
            }
            Tail = newNode;
            Count++;
            return newNode;
        }

        /// <summary>
        /// Очистка списка
        /// </summary>
        public void Clear()
        {
            Head = Tail = null;
            Count = 0;
        }

        /// <summary>
        /// Поиск элемента по индексу
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        /// <returns>Ссылка на элемент</returns>
        public ListNode ElementAt(int index)
        {
            ListNode node;
            //если значение индекса больше половины списка, то ищем с конца
            if (index > Count / 2)
            {
                node = Tail;
                for (int i = 0; i < Count - index - 1; i++)
                    node = node.Prev;
            }
            //иначе с начала
            else
            {
                node = Head;
                for (int i = 0; i < index; i++)
                    node = node.Next;
            }
            return node;
        }

        // Метод расширение для обохода списка от начала
        public IEnumerator<ListNode> GetEnumerator()
        {
            var node = Head;
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }

        public void Serialize(FileStream s)
        {
            var nodes = new Dictionary<ListNode, int>();
            var index = 0;
            foreach (var node in this)
            {
                nodes.Add(node, index);
            }
            using (var sw = new StreamWriter(s))
            {
                sw.WriteLine(Count);
                foreach (var kvp in nodes)
                {
                    var node = kvp.Key;
                    //конвертация в base64, для того чтобы представить данные в одну строку.
                    var dataAsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(node.Data));
                    sw.WriteLine(dataAsBase64);
                    //получение индекса случайного элемента или -1 если Rand null
                    var randId = node.Rand == null ? -1 : nodes[node.Rand];
                    sw.WriteLine(randId);
                }
            }
        }

        public void Deserialize(FileStream s)
        {
            Clear();
            using (var reader = new StreamReader(s))
            {
                var count = int.Parse(reader.ReadLine());
                var nodes = new ListNode[count];
                var nodeRandId = new Dictionary<ListNode, int>();
                count = 0;
                //считываем файл построчно до конца
                while (reader.Peek() >= 0)
                {
                    var data = reader.ReadLine();
                    //ковертируем данные в исходный формат
                    var node = Add(Encoding.UTF8.GetString(Convert.FromBase64String(data)));
                    //считываем индекс случайного элемента
                    var randId = int.Parse(reader.ReadLine());
                    if (randId > -1)
                        nodeRandId.Add(node, randId);
                    nodes[count] = node;
                    count++;
                }
                foreach (var kvp in nodeRandId)
                    kvp.Key.Rand = nodes[kvp.Value];
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var listNode = new ListRand();
            var rnd = new Random();
            var count = 1000000;
            var nodes = new ListNode[count];

            for (int i = 0; i < count; i++)
                nodes[i] = listNode.Add("data" + i.ToString());

            foreach (var node in listNode)
            {
                if (count % 2 == 0)
                    node.Rand = nodes[rnd.Next(0, count)];
                count--;
            }

            using (var fs = new FileStream("serialized.txt", FileMode.Create))
                listNode.Serialize(fs);

            using (var fs = new FileStream("serialized.txt", FileMode.Open))
                listNode.Deserialize(fs);
        }
    }
}

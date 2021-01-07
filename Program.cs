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
                Head = newNode;
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

        /// <summary>
        /// Поиск индекса элемента от начала.
        /// </summary>
        /// <param name="node">Элемент для поиска</param>
        /// <returns>Индекс элемента или -1</returns>
        public int IndexOf(ListNode node)
        {
            if (node == null) return -1;
            var index = 0;
            foreach (var temp in this)
            {
                if (temp == node) return index;
                index++;
            }
            return -1;
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
            using (var sw = new StreamWriter(s))
                foreach (var node in this)
                {
                    //конвертация в base64, для того чтобы представить данные в одну строку.
                    var dataAsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(node.Data));
                    sw.WriteLine(dataAsBase64);
                    //получение индекса случайного элемента от начала списка
                    var randIndex = IndexOf(node.Rand);
                    sw.WriteLine(randIndex);
                }
        }

        public void Deserialize(FileStream s)
        {
            Clear();
            //словарь для хранения индекса случайного элемента
            var randIndexes = new Dictionary<ListNode, int>();
            using (var reader = new StreamReader(s))
            {
                //считываем файл построчно до конца
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    //ковертируем данные в исходный формат
                    var node = Add(Encoding.UTF8.GetString(Convert.FromBase64String(line)));
                    //считываем индекс случайного элемента
                    var randIndex = int.Parse(reader.ReadLine());
                    //если индекс равен -1, значит не было ссылки на случайный элемент
                    if (randIndex > -1) randIndexes.Add(node, randIndex);
                }
            }
            //восстанавливаем ссылки на случайные элементы
            foreach (var kvp in randIndexes)
                kvp.Key.Rand = ElementAt(kvp.Value);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var listNode = new ListRand();
            var rnd = new Random();
            var count = 100000;
            for (int i = 0; i < count; i++)
            {
                var newNode = listNode.Add("data" + i.ToString());
                newNode.Rand = listNode.ElementAt(rnd.Next(0, i + 1));
            }
            using (var fs = new FileStream("serialized.txt", FileMode.Create))
            {
                listNode.Serialize(fs);
            }
            using (var fs = new FileStream("serialized.txt", FileMode.Open))
            {
                listNode.Deserialize(fs);
            }
        }
    }
}

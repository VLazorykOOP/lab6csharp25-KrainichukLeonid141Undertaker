using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Lab6_Interfaces_Exceptions
{

    interface IPrintable { void Show(); }
    interface IDocument : IPrintable
    {
        string Number { get; set; }
        DateTime Date { get; set; }
    }

    class Document : IDocument, ICloneable
    {
        public string Number { get; set; }
        public DateTime Date { get; set; }

        public Document(string n, DateTime d) { Number = n; Date = d; }

        public virtual void Show() => WriteLine($"Документ №{Number} від {Date.ToShortDateString()}");

        public object Clone() => new Document(this.Number, this.Date);
    }

    class Receipt : Document
    {
        public decimal Amount { get; set; }
        public Receipt(string n, DateTime d, decimal a) : base(n, d) => Amount = a;
        public override void Show() => WriteLine($"Квитанція №{Number}: {Amount} грн.");
    }


    class SoftwareExpiredException : Exception
    {
        public SoftwareExpiredException(string message) : base(message) { }
    }

    interface ISoftware : IComparable<ISoftware>
    {
        string Name { get; }
        string Manufacturer { get; }
        void ShowInfo();
        bool IsUsable(DateTime date);
    }

    abstract class BaseSoftware : ISoftware
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }

        protected BaseSoftware(string n, string m) { Name = n; Manufacturer = m; }

        public abstract void ShowInfo();
        public abstract bool IsUsable(DateTime date);

        public int CompareTo(ISoftware other) => string.Compare(this.Name, other?.Name);
    }

    class FreeSoftware : BaseSoftware
    {
        public FreeSoftware(string n, string m) : base(n, m) { }
        public override void ShowInfo() => WriteLine($"[Вільне] {Name} ({Manufacturer})");
        public override bool IsUsable(DateTime date) => true;
    }

    class SharewareSoftware : BaseSoftware
    {
        public DateTime InstallDate { get; set; }
        public int TrialDays { get; set; }
        public SharewareSoftware(string n, string m, DateTime d, int days) : base(n, m)
        { InstallDate = d; TrialDays = days; }

        public override void ShowInfo() => WriteLine($"[Умовно-безкоштовне] {Name}, Тріал: {TrialDays} днів");
        public override bool IsUsable(DateTime date) => (date - InstallDate).TotalDays <= TrialDays;
    }

    class SoftwareLibrary : IEnumerable<ISoftware>
    {
        private List<ISoftware> _items = new List<ISoftware>();

        public void Add(ISoftware soft) => _items.Add(soft);

        public IEnumerator<ISoftware> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class Program
    {
        static void Main()
        {
            WriteLine("=== Завдання 1: Інтерфейси ===");
            Document doc1 = new Document("INV-001", DateTime.Now);
            Document doc2 = (Document)doc1.Clone();
            doc2.Number = "INV-CLONE";
            doc1.Show();
            doc2.Show();

            WriteLine("\n=== Завдання 2 & 4: ПЗ та IEnumerable ===");
            SoftwareLibrary library = new SoftwareLibrary {
                new FreeSoftware("Linux Kernel", "Open Source"),
                new SharewareSoftware("WinRAR", "RARLab", DateTime.Now.AddDays(-50), 40),
                new FreeSoftware("GIMP", "GNU Team")
            };

            foreach (var soft in library)
            {
                soft.ShowInfo();
                WriteLine($"  Можна використовувати: {soft.IsUsable(DateTime.Now)}");
            }

            WriteLine("\n=== Завдання 3: Винятки ===");
            try
            {
                ISoftware expiredSoft = new SharewareSoftware("OldApp", "Legacy", DateTime.Now.AddYears(-1), 30);

                if (!expiredSoft.IsUsable(DateTime.Now))
                    throw new SoftwareExpiredException($"ПЗ '{expiredSoft.Name}' протерміновано!");

                object someObj = "Це рядок, а не документ";
                Document failDoc = (Document)someObj;
            }
            catch (SoftwareExpiredException ex)
            {
                ForegroundColor = ConsoleColor.Yellow;
                WriteLine($"Власний виняток: {ex.Message}");
                ResetColor();
            }
            catch (InvalidCastException ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"Стандартний виняток: Помилка приведення типів! ({ex.Message})");
                ResetColor();
            }
            finally
            {
                WriteLine("Блок finally: Завершення обробки.");
            }

            WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            ReadKey();
        }
    }
}
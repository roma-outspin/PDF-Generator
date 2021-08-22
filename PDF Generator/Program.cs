//Программа для тестирования использования библиотека генерирующей PDF файлы
//Собрана из примеров от создателей библиотеки
//FreeSpire.PDF
//https://www.e-iceblue.com/
//https://github.com/eiceblue/Spire.PDF-for-.NET/tree/master/CS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDF_Generator
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

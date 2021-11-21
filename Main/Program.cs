using System;
using System.Collections.Generic;
using System.IO;
using Bot;
using FileManager;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using urfuQueueBot;
using System.Linq;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            string token;
            using (StreamReader sr = new StreamReader("../../../../.private_cfg/token.txt"))
            {
                token = sr.ReadLine();
            }

            //var roomsTable = new TableIO("1jp7fIEEOrGEVtoFU5dvE3FIwzKzjwMzxKJy5YH04w24");
            //Console.WriteLine(String.Join(", ", (roomsTable.GetAllSheets().Select(sheet => sheet.Name))));

            BotClass bot = new BotClass(token);

            AddGeneralWelcomeResponse(bot);

            AddExpertBranch(bot);
            AddStudentBranch(bot);

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }

        #region General
        private static void AddGeneralWelcomeResponse(BotClass bot)
        {
            using StreamReader sr = new StreamReader("../../../../src/generalWelcome.txt");
            bot.AddResponse(
                null,
                new List<string> { "/start" },
                bot.SendMessage(
                    text: sr.ReadLine(),
                    keyboard: new KeyboardCreator(new List<string> { "Student", "Expert" }).GetReplyMarkup()
                )
            );
        }
        #endregion

        #region Expert branch
        private static void AddExpertBranch(BotClass bot)
        {
            AddExpertWelcomeResponse(bot);

            AddGoogleSheetResponse(bot);
            AddGoogleSheetQuery(bot);
        }

        private static void AddExpertWelcomeResponse(BotClass bot)
        {
            using StreamReader sr = new StreamReader("../../../../src/expertWelcome.txt");
            bot.AddResponse(
                (Message mes) => mes.Text == "Expert",
                null,
                bot.SendMessage(
                    text: sr.ReadLine(),
                    keyboard: new KeyboardCreator(new List<string> { "Получить коды комнат", "Шаблон гугл таблицы" }).GetReplyMarkup()
                )
            );
        }

        private static void AddGoogleSheetResponse(BotClass bot)
        {
            bot.AddResponse(
                mes => mes.Text == "Шаблон гугл таблицы",
                null,
                bot.SendMessage(
                    files: new List<IFile>
                    {
                        new FileManager.File(FileType.Document, "Описание шаблона", "../../../../src/Описание шаблона.xlsx"),
                        new FileManager.File(FileType.Image, "Пример шаблона", "../../../../src/Пример шаблона.jpg"),
                        new FileManager.File(FileType.Document, "Пример шаблона", "../../../../src/Пример шаблона.xlsx")
                    },
                    keyboard: new KeyboardCreator(new List<string> { "Получить коды комнат", "Шаблон гугл таблицы" })
                        .GetReplyMarkup())
                );
        }

        private static void AddGoogleSheetQuery(BotClass bot)
        {
            bot.AddResponse(
                (Message mes) => mes.Text == "Получить коды комнат",
                null,
                bot.AddQuery("Введите код комнаты",
                             bot.GetRoomsResponse()
                )
            );
        }
        #endregion

        #region Student
        private static void AddStudentBranch(BotClass bot)
        {
            AddStudentWelcomeResponse(bot);

            AddConnectQuery(bot);
        }

        private static void AddConnectQuery(BotClass bot)
        {
            bot.AddResponse(
                (Message mes) => mes.Text == "Подключение к комнате",
                null,
                bot.SendMessage("Введите, пожалуйста, код, который вам выдали ваши эксперты. (Стикер)")
            );
        }

        private static void AddStudentWelcomeResponse(BotClass bot)
        {
            using StreamReader sr = new StreamReader("../../../../src/studentWelcome.txt");
            bot.AddResponse(
                (Message mes) => mes.Text == "Student",
                null,
                bot.SendMessage(
                    text: sr.ReadLine(),
                    keyboard: new KeyboardCreator(new List<string> { "Подключение к комнате" }).GetReplyMarkup()
                )
            );
        }
        #endregion

        private void GetRooms()
        {

        }
    }
}

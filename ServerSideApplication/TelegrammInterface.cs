using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace ServerSideApplication
{
    internal static class TelegrammInterface
    {
        private static TelegramBotClient bot = new TelegramBotClient("5375160115:AAF_4mLWdxo3RyBlvlhjhYqjsFonc3CiRwM");
        private static List<MonitoringServerConnection> _appliedConnections = new List<MonitoringServerConnection>();
        private static List<CurrentConnection> ChoosenConnections = new List<CurrentConnection>();
        
        //chatId + name 
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));


            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),            
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                _ => UnknownUpdateHandlerAsync(botClient, update),
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }


        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static void Source(List<MonitoringServerConnection> AppliedConnections)
        {
            _appliedConnections = AppliedConnections;
            Console.WriteLine("Telegram bot is up, bot name -  " + bot.GetMeAsync().Result.FirstName); 
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == null)
                return;

            switch (message.Text) 
            {
                case "/start": GetAvailableServers(botClient, message); break;
                case "ОЗУ": GetMonotoringParametr(botClient, message, 0); break;
                case "Имя устройства": GetMonotoringParametr(botClient, message, 1); break;
                case "Интерфейсы и трафик": GetMonotoringParametr(botClient, message, 2); break;
                case "ЦПУ": GetMonotoringParametr(botClient, message, 3); break;
                case "Дисковое пространство": GetMonotoringParametr(botClient, message, 4); break;
                case "Сервисы": GetMonotoringParametr(botClient, message, 5); break;
                case "Вернуться к выбору сервера": ClearCurrentConnections(botClient, message); break;
            }


    
         /*   if (message.Text.ToLower() == "/start")
            {
                GetAvailableServers(botClient, message);
                return;
            }*/
        }

        private async static void GetMonotoringParametr(ITelegramBotClient botClient, Message message, int MonitoringIndex)
        {
            foreach (var CurrentConnection in ChoosenConnections)
            {
                if(CurrentConnection.ChatId == message.Chat.Id)
                {
                    string MonitoringData = CurrentConnection.Connection.RequestMonitoringData(MonitoringIndex);
                    await botClient.SendTextMessageAsync(message.Chat.Id, MonitoringData);
                    return;
                }
            }
        }

        private static void ClearCurrentConnections(ITelegramBotClient botClient, Message message)
        {
            foreach (var CurrentConnection in ChoosenConnections)
            {
                if (CurrentConnection.ChatId == message.Chat.Id)
                {
                    ChoosenConnections.Remove(CurrentConnection);
                    break;
                }
            }
            Console.WriteLine("asgfoiuasgopifgasoifiasofgasgfoas");
            GetAvailableServers(botClient, message);
        }

        private async static void GetAvailableServers(ITelegramBotClient botClient, Message _message)
        {
            //List<KeyboardButton> KeyBoard = new List<KeyboardButton>();
            //  _appliedConnections[0].RequestMonitoringData(1);
            foreach (var ServerMonitorConnection in _appliedConnections)
            {
                string MachinName = ServerMonitorConnection.RequestMonitoringData(1);

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] // first row
                    {
                        InlineKeyboardButton.WithCallbackData("Просмотреть параметры мониторинга", MachinName),
                    }
                });

                await botClient.SendTextMessageAsync(_message.Chat.Id, $"Server - {MachinName}", replyMarkup: keyboard);
            }
        }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}");

        /*    await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}");*/

            await SendReplyKeyboard(botClient, callbackQuery.Message!,  callbackQuery.Data!);
        }


        private static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, string TextToReply)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = 
            new
            (
                new[]
                {
                    new KeyboardButton[] { "ОЗУ", "Имя устройства" },
                    new KeyboardButton[] { "Интерфейсы и трафик", "ЦПУ"},
                    new KeyboardButton[] { "Дисковое пространство", "Сервисы"},
                    new KeyboardButton[] {"Вернуться к выбору сервера"}
                }
            )
            {
                ResizeKeyboard = true
            };

            SetCurrentMonitoringServer(botClient, message, TextToReply);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: TextToReply, replyMarkup: replyKeyboardMarkup);

        }


        private static void SetCurrentMonitoringServer(ITelegramBotClient botClient, Message message,string ServerName)
        {
            foreach (var Server in _appliedConnections)
            {
                if(Server.RequestMonitoringData(1) == ServerName)
                {
                    ChoosenConnections.Add(new CurrentConnection { ChatId = message.Chat.Id, Connection = Server }) ;
                }           
            }      
        }
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

    }

    class CurrentConnection
    {
        public long ChatId { get; set; }
        public MonitoringServerConnection Connection { get; set; }
    }

}

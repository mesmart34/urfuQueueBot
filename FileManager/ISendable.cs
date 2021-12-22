﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace FileManager
{
    public interface ISendable
    {
        Task Send(ITelegramBotClient client, ChatId id);
    }
}

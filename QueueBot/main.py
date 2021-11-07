# -*- coding: utf-8 -*-

from bot.QueueBot import QueueBot
from bot.File import *
from bot.FileGetter import *
import config as cfg
import pathlib

if __name__ == '__main__':
    welcome = open('src/welcome.txt', 'r', encoding='utf-8').readline()

    _bot = QueueBot(cfg.token)

    # === === === === command: '/start'
    buttons = [
        _bot.get_button('Получить коды комнат'),
        _bot.get_button('Шаблон гугл таблицы')
    ]
    keyboard = _bot.get_keyboard(buttons)

    _bot.add_command('start',
                     _bot.send_message(welcome, keyboard=keyboard))

    # === === === === buttons' responses
    _bot.add_response('Шаблон гугл таблицы',
                      _bot.send_message(files=[
                                            File('src\\Описание шаблона.xlsx', 'document'),
                                            File('src\\Пример шаблона.jpg', 'photo', 'Пример шаблона'),
                                            File('src\\Пример шаблона.xlsx', 'document')
                                        ],
                                        keyboard=keyboard))

    _bot.add_response('Получить коды комнат',
                      _bot.send_query('Введите ссылку на таблицу',
                                      _bot.send_message('GOT IT')))

    _bot.start()

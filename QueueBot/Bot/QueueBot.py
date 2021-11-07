import telebot
from telebot import types
from bot import File


class QueueBot:
    __query_queue = []

    __type_methods = {
        'photo': telebot.TeleBot.send_photo,
        'document': telebot.TeleBot.send_document
    }

    def __in_queue(self, message) -> bool:
        if message.chat.id in self.__query_queue:
            self.__query_queue.remove(message.chat.id)
            return True
        return False

    def __init__(self, token: str):
        self.__bot = telebot.TeleBot(token)

    def add_command(self, name: str, action: callable):
        self.__bot.message_handler(commands=[name])(action)

    def add_response(self, text_to_response: str, response: callable):
        self.__bot.message_handler(func=lambda mes: mes.text == text_to_response)(response)

    def add_waiting(self, response):
        self.__bot.message_handler(func=lambda mes: self.__in_queue(mes))(response)

    def send_message(self, text: str = '', files: list = None, keyboard=None):
        def wrapped(message: types.Message):
            if text != '':
                self.__bot.send_message(
                    message.chat.id,
                    text,
                    reply_markup=(keyboard if keyboard is not None else types.ReplyKeyboardRemove())
                )
            if files is not None and len(files) > 0:
                for file in files:
                    f = open(file.get_path(), 'rb')
                    self.__type_methods[file.get_type()](self.__bot, message.chat.id, f)

        return wrapped

    def send_query(self, query_text: str, query_response):
        def wrapped(message: types.Message):
            self.__query_queue.append(message.chat.id)
            self.__bot.send_message(message.chat.id, query_text)

        self.add_waiting(query_response)
        return wrapped

    @staticmethod
    def get_keyboard(buttons: list):
        markup = types.ReplyKeyboardMarkup(row_width=2)
        markup.add(*buttons)
        return markup

    @staticmethod
    def get_button(name: str):
        return types.KeyboardButton(name)

    def start(self):
        self.__bot.infinity_polling()

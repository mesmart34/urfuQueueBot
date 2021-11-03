import telebot
from telebot import types


class QueueBot:

    def __init__(self):
        raise NotImplementedError

    def add_command(self, name: str, action):
        raise NotImplementedError

    def get_keyboard(self, buttons: list):
        raise NotImplementedError

    def get_button(self, name: str, action):
        raise NotImplementedError

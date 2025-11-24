// === НОВОГОДНИЙ ДИАЛОГ С МЫШОНКОМ ===

VAR has_cheese = false
VAR has_acorn = false

-> start

=== start ===
Мышонок: Пи-пи! С Новым Годом!#c:yellow
Мышонок: Помоги собрать подарки? Нужен сыр и жёлудь!#c:yellow

+ [Помогу! Что у тебя есть?] -> check_gifts
+ [Извини, спешу] -> refuse

=== refuse ===
Мышонок: Понятно... Удачи тебе!#c:red
-> END

=== check_gifts ===
Мышонок: Что принёс?#c:yellow

+ {not has_cheese} [Держи сыр!] -> give_cheese
+ {not has_acorn} [Вот жёлудь!] -> give_acorn
+ {has_cheese and has_acorn} [Всё собрал!] -> complete

=== give_cheese ===
~ has_cheese = true
Мышонок: Ммм, ароматный! Мама обрадуется!#c:green
{ has_acorn:
    -> check_complete
- else:
    Мышонок: Осталось найти жёлудь!#c:yellow
    -> check_gifts
}
 === give_acorn ===
~ has_acorn = true
Мышонок: Какой крепкий! Папе понравится!#c:green
{ has_cheese:
    -> check_complete
- else:
    Мышонок: Теперь нужен сыр!#c:yellow
    -> check_gifts
}

=== check_complete ===
Мышонок: Ура! Всё нашёл! Спасибо!#c:green
-> complete

=== complete ===
Мышонок: За помощь дарю волшебную снежинку!#c:yellow
Мышонок: Она исполнит новогоднее желание!#c:green

+ [Ого! Спасибо большое!] -> take_gift
+ [Может лучше оставь себе?] -> share_gift

=== take_gift ===
Мышонок: Пользуйся! С Новым Годом!#c:green
-> END

=== share_gift ===
Мышонок: Какой добрый! Загадаем желание вместе!#c:green
Мышонок: Пусть все будут счастливы! Пи-пи!#c:yellow
-> END
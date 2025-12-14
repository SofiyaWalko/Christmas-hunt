// === НОВОГОДНИЙ ДИАЛОГ С МЫШОНКОМ ===

VAR has_cheese = false
VAR has_acorn = false
VAR statCount = 0
VAR hasStat = false
VAR hasEnoughStats = false

-> start

=== start ===
Местный житель: Привет! Ты, наверное, мышонок Белл и помогаешь Санте собрать подарки?#c:yellow

+ [Да] -> answer

=== answer ===
{ hasEnoughStats:
    Местный житель: Я с радостью помогу тебе найти очень важный подарок. Тебе необходимо пройти прямо.#c:green
 - else:
    Местный житель: Я с радостью помогу тебе найти очень важный подарок. Тебе необходимо пройти налево.#c:red
}

+ [Спасибо!] -> end

=== end ===
Местный житель: Удачи!#c:yellow
-> END
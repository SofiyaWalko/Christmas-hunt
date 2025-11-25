// === НОВОГОДНИЙ ДИАЛОГ С МЫШОНКОМ ===

VAR has_cheese = false
VAR has_acorn = false
VAR statCount = 0
VAR hasStat = false
VAR hasEnoughStats = false

-> start

=== start ===
Шишка: Привет!#c:yellow

{ hasEnoughStats:
    Шишка: У тебя уже {statCount} стат(ов) — этого достаточно, спасибо!#c:green
 - else:
    Шишка: У тебя всего {statCount} стат(ов). Нужно больше, чтобы я доверил задание.#c:red
}

+ [Понятно] -> end

=== end ===
Шишка: Удачи!#c:yellow
-> END
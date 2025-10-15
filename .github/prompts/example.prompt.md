---
mode: 'agent'
description: 'Generuj testy jednostkowe do bieżącego pliku C#'
---

Wygeneruj komplet testów xUnit dla pliku `${fileBasename}` (C#) na podstawie aktualnej zawartości projektu.

Wymagania:
- użj xUnit
- nazwij klasę testową `${fileBasenameNoExtension}Tests`
- pokryj przypadki brzegowe i wyjątki
- jeżeli są zależności, zaproponuj mocki (np. Moq)

Jeżeli zaznaczono kod, generuj testy tylko dla zaznaczonych metod:
${selection}

Jeżeli potrzebujesz dodatkowych informacji, zadaj pytania.

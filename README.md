# DailyPlanerV3

Приложение-ежедневник для планирования личного времени: задачи, события,
заметки, напоминания, календарь и анализ продуктивности.

## Технологии

- Платформа: .NET Framework 4.8
- Язык: C#
- Интерфейс: WPF, шаблон MVVM
- База данных: Microsoft SQL Server, доступ через Entity Framework 6
- Среда разработки: Visual Studio 2022 Community

## Структура проекта

```
DailyPlanerV3/
├── DailyPlaner.sln            – файл решения
├── README.md                  – описание проекта
├── ARTIFACTS.md                – перечень артефактов и протоколов
├── .gitignore                  – исключения для Git
└── DailyPlaner/                – основной проект приложения
    ├── App.xaml / App.xaml.cs  – точка входа, ресурсы приложения
    ├── Models/                 – сущности: User, Task, Event, Note, Reminder, Role, Gender
    ├── Views/                  – окна LoginWindow, MainWindow, RegisterWindow
    │   └── Dialogs/            – диалоги TaskDialog, EventDialog, NoteDialog, DetailsDialog
    ├── ViewModels/             – модели представления LoginViewModel, MainViewModel, RegisterViewModel
    ├── Services/               – сервисы DatabaseService, NotificationService, ReminderScheduler
    ├── Infrastructure/         – RelayCommand, конвертеры значений
    └── Database/               – скрипт создания базы данных DailyPlannerDB.sql
```

## Назначение каталогов

- **Models** – классы сущностей предметной области, не зависят от интерфейса.
- **Views** – окна и диалоги приложения, разметка XAML и код взаимодействия.
- **ViewModels** – логика представления: свойства для привязок, команды.
- **Services** – бизнес-логика: работа с базой данных, напоминания, уведомления.
- **Infrastructure** – вспомогательные механизмы MVVM: RelayCommand и конвертеры.
- **Database** – SQL-скрипты и модель доступа к данным.

## Сборка и запуск

1. Установить Visual Studio 2022 с рабочей нагрузкой «Разработка классических приложений .NET».
2. Восстановить пакеты NuGet.
3. Выполнить скрипт Database/DailyPlannerDB.sql в Microsoft SQL Server.
4. Указать строку подключения в App.config.
5. Собрать решение и запустить проект DailyPlaner.

## Репозиторий

Проект версионируется в Git. Основная ветка – main, изменения фиксируются
осмысленными сообщениями коммитов.

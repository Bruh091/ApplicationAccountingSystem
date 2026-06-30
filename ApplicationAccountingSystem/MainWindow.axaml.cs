using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ApplicationAccountingSystem.Domain.Designation;

namespace ApplicationAccountingSystem
{
    public partial class MainWindow : Window
    {
        // ----- session state -----
        private UserRole _currentRole;
        private string _currentUsername = "";
        private bool _isLightTheme = true;
        private string _lastPanel = "";

        // ----- in-memory storage (stub until DB is connected) -----
        private readonly List<InMemoryUser> _users = new();
        private readonly List<InMemoryTicket> _tickets = new();
        private readonly List<InMemoryComment> _comments = new();

        private readonly ObservableCollection<TicketListItem> _myTicketItems = new();
        private readonly ObservableCollection<TicketListItem> _queueItems = new();
        private readonly ObservableCollection<UserListItem> _userItems = new();
        private readonly ObservableCollection<CommentListItem> _detailCommentItems = new();

        private Guid _selectedTicketId = Guid.Empty;

        // ----- users pagination -----
        private int _userColumns = 1;
        private int UsersPerPage => Math.Max(1, _userColumns) * 4;
        private readonly List<UserListItem> _allUserItems = new();
        private int _currentUserPage = 1;
        private int _totalUserPages = 1;

        // ----- navigation -----
        private readonly List<NavEntry> _allNavEntries = new()
        {
            new("📊 Панель управления", "Dashboard", UserRole.Admin),
            new("📝 Создать тикет", "CreateTicket", UserRole.User),
            new("📋 Мои тикеты", "MyTickets", UserRole.User),
            new("📥 Очередь", "Queue", UserRole.Agent),
            new("👥 Пользователи", "Users", UserRole.Admin),
            new("⚙️ SLA политики", "SLA", UserRole.User),
        };

        public MainWindow()
        {
            InitializeComponent();

            // seed demo users
            _users.Add(new InMemoryUser { Username = "admin", Password = "admin", FullName = "Иван Админов", Role = UserRole.Admin });
            _users.Add(new InMemoryUser { Username = "agent", Password = "agent", FullName = "Пётр Агентов", Role = UserRole.Agent });
            _users.Add(new InMemoryUser { Username = "user", Password = "user", FullName = "Сергей Клиентов", Role = UserRole.User });

            for (int i = 1; i <= 50; i++)
                _users.Add(new InMemoryUser { Username = $"test{i}", Password = "test", FullName = $"Тестовый {i}", Role = UserRole.User });

            // seed demo tickets
            _tickets.Add(new InMemoryTicket
            {
                Id = Guid.NewGuid(),
                Title = "Не запускается приложение после обновления",
                Description = "После обновления до версии 2.1 программа выдаёт ошибку при запуске. Ошибка: 'System.IO.FileNotFoundException'",
                Status = TicketStatus.New,
                Priority = TicketPriority.High,
                CreatedAt = DateTime.Now.AddHours(-3),
                CreatorName = "user",
            });
            _tickets.Add(new InMemoryTicket
            {
                Id = Guid.NewGuid(),
                Title = "Сбросить пароль администратора",
                Description = "Необходимо сбросить пароль для учётной записи администратора отдела продаж",
                Status = TicketStatus.New,
                Priority = TicketPriority.Medium,
                CreatedAt = DateTime.Now.AddHours(-8),
                CreatorName = "user",
            });
            _tickets.Add(new InMemoryTicket
            {
                Id = Guid.NewGuid(),
                Title = "Настроить почтовый сервер",
                Description = "SMTP-сервер не отправляет письма с уведомлениями. Нужно проверить конфигурацию.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Urgent,
                CreatedAt = DateTime.Now.AddDays(-1),
                CreatorName = "user",
                AssigneeName = "agent",
            });

            // subscribe to theme changes
            App.ThemeChanged += OnThemeChanged;

            // recalculate user columns on resize
            this.SizeChanged += OnMainWindowSizeChanged;
        }

        // ====================================================================
        //  LOGIN / REGISTER
        // ====================================================================

        private void LoginBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = LoginUsername.Text?.Trim();
            var password = LoginPassword.Text?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Заполните все поля");
                return;
            }

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null && user.Password == password)
            {
                LoginSuccess(user);
            }
            else
            {
                ShowLoginError("Неверное имя пользователя или пароль");
            }
        }

        private void RegisterBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = LoginUsername.Text?.Trim();
            var password = LoginPassword.Text?.Trim();
            var roleIndex = LoginRoleCombo.SelectedIndex;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Заполните все поля");
                return;
            }

            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                ShowLoginError("Пользователь уже существует");
                return;
            }

            var role = roleIndex switch
            {
                0 => UserRole.Admin,
                1 => UserRole.Agent,
                _ => UserRole.User,
            };

            var newUser = new InMemoryUser
            {
                Username = username,
                Password = password,
                FullName = username,
                Role = role,
            };
            _users.Add(newUser);

            LoginSuccess(newUser);
        }

        private void LoginSuccess(InMemoryUser user)
        {
            _currentRole = user.Role;
            _currentUsername = user.Username;

            LoginPanel.IsVisible = false;
            MainWorkspace.IsVisible = true;

            UserInfoBar.Text = $"{user.FullName} ({RoleDisplayName(user.Role)})";

            // DB stub: загрузка тикетов из БД
            // TODO: раскомментировать после настройки БД
            // using (var db = new AppDbContext())
            // {
            //     var tickets = db.Tickets.Where(t => t.CreatedById == userId || t.AssignedToId == userId).ToList();
            //     ...
            // }

            PopulateNav(user.Role);
            RefreshDashboard();
        }

        private void LogoutBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _currentUsername = "";
            _currentRole = UserRole.User;

            LoginUsername.Text = "";
            LoginPassword.Text = "";
            LoginError.IsVisible = false;

            MainWorkspace.IsVisible = false;
            LoginPanel.IsVisible = true;
        }

        // ====================================================================
        //  NAVIGATION
        // ====================================================================

        private void PopulateNav(UserRole role)
        {
            NavList.Items.Clear();

            var entries = _allNavEntries
                .Where(n => role <= n.MinRole)
                .OrderBy(n => n.MinRole)
                .ThenBy(n => n.Label);

            foreach (var entry in entries)
            {
                var item = new ListBoxItem
                {
                    Content = $"{entry.Icon}  {entry.Label}",
                    Tag = entry.Tag,
                };
                NavList.Items.Add(item);
            }

            if (NavList.Items.Count > 0)
                NavList.SelectedIndex = 0;
        }

        private void NavList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem item && item.Tag is string tag)
            {
                ShowPanel(tag);
            }
        }

        private void ShowPanel(string tag)
        {
            // hide all
            DashboardPanel.IsVisible = false;
            CreateTicketPanel.IsVisible = false;
            MyTicketsPanel.IsVisible = false;
            QueuePanel.IsVisible = false;
            UsersPanel.IsVisible = false;
            SLAPanel.IsVisible = false;
            TicketDetailPanel.IsVisible = false;

            _lastPanel = tag;

            switch (tag)
            {
                case "Dashboard":
                    DashboardPanel.IsVisible = true;
                    RefreshDashboard();
                    break;
                case "CreateTicket":
                    CreateTicketPanel.IsVisible = true;
                    break;
                case "MyTickets":
                    MyTicketsPanel.IsVisible = true;
                    RefreshMyTickets();
                    break;
                case "Queue":
                    QueuePanel.IsVisible = true;
                    RefreshQueue();
                    break;
                case "Users":
                    UsersPanel.IsVisible = true;
                    RefreshUsers();
                    break;
                case "SLA":
                    SLAPanel.IsVisible = true;
                    var isAdmin = _currentRole == UserRole.Admin;
                    SLALowResponse.IsReadOnly = !isAdmin;
                    SLALowResolution.IsReadOnly = !isAdmin;
                    SLAMediumResponse.IsReadOnly = !isAdmin;
                    SLAMediumResolution.IsReadOnly = !isAdmin;
                    SLAHighResponse.IsReadOnly = !isAdmin;
                    SLAHighResolution.IsReadOnly = !isAdmin;
                    SLACriticalResponse.IsReadOnly = !isAdmin;
                    SLACriticalResolution.IsReadOnly = !isAdmin;
                    SaveSLABtn.IsVisible = isAdmin;
                    break;
            }
        }

        // ====================================================================
        //  THEME
        // ====================================================================

        private void ThemeToggleBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            App.ToggleTheme();
        }

        private void OnThemeChanged(object? sender, bool isLight)
        {
            _isLightTheme = isLight;
            ThemeToggleBtn.Content = isLight ? "🌙" : "☀️";

            // Update custom brushes for the theme
            if (isLight)
            {
                Resources["SidebarBrush"] = new SolidColorBrush(Color.Parse("#FAFAFA"));
                Resources["ContentBgBrush"] = new SolidColorBrush(Color.Parse("#F5F5F5"));
                Resources["CardBgBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                Resources["BorderBrush"] = new SolidColorBrush(Color.Parse("#E0E0E0"));
                Resources["TextMutedBrush"] = new SolidColorBrush(Color.Parse("#888888"));
            }
            else
            {
                Resources["SidebarBrush"] = new SolidColorBrush(Color.Parse("#252525"));
                Resources["ContentBgBrush"] = new SolidColorBrush(Color.Parse("#1A1A1A"));
                Resources["CardBgBrush"] = new SolidColorBrush(Color.Parse("#2D2D2D"));
                Resources["BorderBrush"] = new SolidColorBrush(Color.Parse("#404040"));
                Resources["TextMutedBrush"] = new SolidColorBrush(Color.Parse("#999999"));
            }
        }

        // ====================================================================
        //  DASHBOARD
        // ====================================================================

        private void RefreshDashboard()
        {
            var openCount = _tickets.Count(t => t.Status == TicketStatus.New);
            var inProgressCount = _tickets.Count(t => t.Status == TicketStatus.InProgress);
            var waitCount = _tickets.Count(t => t.Status == TicketStatus.waitingForCustomer);
            var resolvedCount = _tickets.Count(t => t.Status == TicketStatus.Resolved);
            var closedCount = _tickets.Count(t => t.Status == TicketStatus.Closed);

            StatTotalTickets.Text = _tickets.Count.ToString();
            StatOpenTickets.Text = openCount.ToString();
            StatInProgressTickets.Text = inProgressCount.ToString();
            StatWaitingTickets.Text = waitCount.ToString();
            StatResolvedTickets.Text = resolvedCount.ToString();
            StatClosedTickets.Text = closedCount.ToString();

            // stub for avg resolution time
            StatAvgResolution.Text = "— (данные появятся после подключения БД)";
        }

        // ====================================================================
        //  CREATE TICKET
        // ====================================================================

        private void CreateTicketBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var title = TicketTitle.Text?.Trim();
            var description = TicketDescription.Text?.Trim();

            if (string.IsNullOrEmpty(title))
            {
                CreateTicketError.Text = "Введите тему тикета";
                CreateTicketError.IsVisible = true;
                return;
            }

            var priority = TicketPriorityCombo.SelectedIndex switch
            {
                0 => TicketPriority.Low,
                1 => TicketPriority.Medium,
                2 => TicketPriority.High,
                3 => TicketPriority.Urgent,
                _ => TicketPriority.Medium,
            };

            var ticket = new InMemoryTicket
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description ?? "",
                Status = TicketStatus.New,
                Priority = priority,
                CreatedAt = DateTime.Now,
                CreatorName = _currentUsername,
            };
            _tickets.Add(ticket);

            // DB stub: сохранение тикета в БД
            // TODO: раскомментировать после настройки БД
            // using (var db = new AppDbContext())
            // {
            //     db.Tickets.Add(ticket);
            //     db.SaveChanges();
            // }

            TicketTitle.Text = "";
            TicketDescription.Text = "";
            CreateTicketError.IsVisible = false;

            RefreshDashboard();
            ShowPanel("MyTickets");
        }

        // ====================================================================
        //  MY TICKETS
        // ====================================================================

        private void RefreshMyTickets()
        {
            _myTicketItems.Clear();

            // Admin и Agent видят все тикеты, User — только свои
            var myTickets = _currentRole <= UserRole.Agent
                ? _tickets.OrderByDescending(t => t.CreatedAt)
                : _tickets
                    .Where(t => t.CreatorName == _currentUsername || t.AssigneeName == _currentUsername)
                    .OrderByDescending(t => t.CreatedAt);

            foreach (var t in myTickets)
            {
                _myTicketItems.Add(MapToItem(t));
            }

            MyTicketsList.ItemsSource = _myTicketItems;
        }

        private void MyTicketsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MyTicketsList.SelectedItem is TicketListItem item)
            {
                ShowTicketDetail(item.Id);
            }
        }

        // ====================================================================
        //  QUEUE
        // ====================================================================

        private void RefreshQueue()
        {
            _queueItems.Clear();

            var openTickets = _tickets
                .Where(t => t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedAt);

            foreach (var t in openTickets)
            {
                var viewItem = MapToItem(t);
                viewItem.AssignCommand = new RelayCommand(() => AssignTicket(t.Id));
                _queueItems.Add(viewItem);
            }

            QueueList.ItemsSource = _queueItems;
        }

        private void QueueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (QueueList.SelectedItem is TicketListItem item)
            {
                ShowTicketDetail(item.Id);
            }
        }

        private void AssignTicket(Guid ticketId)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null) return;

            // Agent не может взять свой собственный тикет
            if (_currentRole == UserRole.Agent && ticket.CreatorName == _currentUsername)
            {
                return;
            }

            ticket.AssigneeName = _currentUsername;
            ticket.Status = TicketStatus.InProgress;
            // DB stub: обновление в БД
            // TODO: раскомментировать после настройки БД
            // using (var db = new AppDbContext())
            // {
            //     db.Tickets.Update(ticket);
            //     db.SaveChanges();
            // }
            RefreshQueue();
            RefreshDashboard();
        }

        // ====================================================================
        //  USERS
        // ====================================================================

        private void RefreshUsers()
        {
            _allUserItems.Clear();
            foreach (var u in _users)
            {
                var item = new UserListItem
                {
                    Initials = u.FullName.Length >= 2 ? u.FullName[..2].ToUpper() : u.FullName.ToUpper(),
                    FullName = u.FullName,
                    Role = RoleDisplayName(u.Role),
                };
                // DB stub: редактирование пользователя будет после подключения БД
                // TODO: раскомментировать после настройки БД
                // item.EditCommand = new RelayCommand(() => EditUser(u.Username));
                _allUserItems.Add(item);
            }

            RecalcUserColumns();
            _currentUserPage = 1;
            _totalUserPages = Math.Max(1, (int)Math.Ceiling((double)_allUserItems.Count / UsersPerPage));
            ApplyUserPage();
        }

        private void RecalcUserColumns()
        {
            const double cardWidth = 200; // 180 width + 10 left + 10 right
            double w = UsersPanel.Bounds.Width - 20;
            if (w <= 0)
                w = Math.Max(600, Bounds.Width - 240); // fallback: window minus sidebar
            _userColumns = Math.Max(1, (int)(w / cardWidth));
        }

        private void ApplyUserPage()
        {
            // recalc total pages in case columns changed
            _totalUserPages = Math.Max(1, (int)Math.Ceiling((double)_allUserItems.Count / UsersPerPage));
            if (_currentUserPage > _totalUserPages) _currentUserPage = _totalUserPages;
            if (_currentUserPage < 1) _currentUserPage = 1;

            _userItems.Clear();
            var pageItems = _allUserItems
                .Skip((_currentUserPage - 1) * UsersPerPage)
                .Take(UsersPerPage)
                .ToList();

            foreach (var item in pageItems)
                _userItems.Add(item);

            UsersItemsControl.ItemsSource = _userItems;

            // update page info
            UsersPageInfo.Text = $"Страница {_currentUserPage} из {_totalUserPages} • всего {_allUserItems.Count} пользователей";

            // update page buttons
            UsersPrevPageBtn.IsEnabled = _currentUserPage > 1;
            UsersNextPageBtn.IsEnabled = _currentUserPage < _totalUserPages;

            UsersPageButtons.Items.Clear();
            for (int i = 1; i <= _totalUserPages; i++)
            {
                var btn = new Button
                {
                    Content = i.ToString(),
                    Width = 36,
                    Height = 36,
                    FontSize = 13,
                    Tag = i,
                };
                if (i == _currentUserPage)
                {
                    btn.Background = Avalonia.Media.Brush.Parse("#1565C0");
                    btn.Foreground = Avalonia.Media.Brushes.White;
                }
                int page = i;
                btn.Click += (_, _) =>
                {
                    _currentUserPage = page;
                    ApplyUserPage();
                };
                UsersPageButtons.Items.Add(btn);
            }
        }

        private void OnMainWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (UsersPanel.IsVisible)
            {
                RecalcUserColumns();
                ApplyUserPage();
            }
        }

        private void UsersPrevPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_currentUserPage > 1)
            {
                _currentUserPage--;
                ApplyUserPage();
            }
        }

        private void UsersNextPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_currentUserPage < _totalUserPages)
            {
                _currentUserPage++;
                ApplyUserPage();
            }
        }

        private void UsersGoPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (int.TryParse(UsersPageJump.Text?.Trim(), out int page) && page >= 1 && page <= _totalUserPages)
            {
                _currentUserPage = page;
                ApplyUserPage();
            }
            UsersPageJump.Text = "";
        }

        // ====================================================================
        //  TICKET DETAIL
        // ====================================================================

        private void ShowTicketDetail(Guid ticketId)
        {
            _selectedTicketId = ticketId;

            // hide all content panels
            DashboardPanel.IsVisible = false;
            CreateTicketPanel.IsVisible = false;
            MyTicketsPanel.IsVisible = false;
            QueuePanel.IsVisible = false;
            UsersPanel.IsVisible = false;
            SLAPanel.IsVisible = false;
            TicketDetailPanel.IsVisible = true;

            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null) return;

            DetailTitle.Text = ticket.Title;
            DetailStatus.Text = StatusDisplayName(ticket.Status);
            DetailPriority.Text = PriorityDisplayName(ticket.Priority);
            DetailCreator.Text = ticket.CreatorName;
            DetailAssignee.Text = ticket.AssigneeName ?? "—";
            DetailDescription.Text = ticket.Description;

            // set combo boxes to current ticket values
            SelectComboByTag(DetailStatusCombo, ticket.Status.ToString());
            SelectComboByTag(DetailPriorityCombo, ticket.Priority.ToString());

            // show actions panel based on role permissions
            if (_currentRole <= UserRole.Agent)
            {
                // Admin & Agent: can change both status and priority
                DetailActionsPanel.IsVisible = true;
                DetailStatusCombo.IsEnabled = true;
                DetailApplyStatusBtn.IsVisible = true;
                DetailPriorityCombo.IsEnabled = true;
                DetailApplyPriorityBtn.IsVisible = true;
            }
            else
            {
                // Client: can change priority only on their own tickets
                bool isOwn = ticket.CreatorName == _currentUsername;
                DetailActionsPanel.IsVisible = isOwn;
                DetailStatusCombo.IsEnabled = false;
                DetailApplyStatusBtn.IsVisible = false;
                DetailPriorityCombo.IsEnabled = isOwn;
                DetailApplyPriorityBtn.IsVisible = isOwn;
            }

            _detailCommentItems.Clear();
            var ticketComments = _comments.Where(c => c.TicketId == ticketId).OrderBy(c => c.CreatedAt);
            foreach (var c in ticketComments)
            {
                _detailCommentItems.Add(new CommentListItem
                {
                    Author = c.Author,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                });
            }
            DetailComments.ItemsSource = _detailCommentItems;
        }

        private void DetailBackBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ShowPanel(_lastPanel);
        }

        private void DetailApplyStatusBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_selectedTicketId == Guid.Empty) return;
            if (DetailStatusCombo.SelectedItem is not ComboBoxItem item) return;

            var newStatus = item.Tag?.ToString();
            if (newStatus == null) return;

            var ticket = _tickets.FirstOrDefault(t => t.Id == _selectedTicketId);
            if (ticket == null) return;

            if (!Enum.TryParse<TicketStatus>(newStatus, out var parsed)) return;
            ticket.Status = parsed;

            // DB stub
            // TODO: раскомментировать после настройки БД

            ShowTicketDetail(_selectedTicketId);
        }

        private void DetailApplyPriorityBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_selectedTicketId == Guid.Empty) return;
            if (DetailPriorityCombo.SelectedItem is not ComboBoxItem item) return;

            var newPriority = item.Tag?.ToString();
            if (newPriority == null) return;

            var ticket = _tickets.FirstOrDefault(t => t.Id == _selectedTicketId);
            if (ticket == null) return;

            if (!Enum.TryParse<TicketPriority>(newPriority, out var parsed)) return;
            ticket.Priority = parsed;

            // DB stub
            // TODO: раскомментировать после настройки БД

            ShowTicketDetail(_selectedTicketId);
        }

        private static void SelectComboByTag(ComboBox combo, string tag)
        {
            for (int i = 0; i < combo.ItemCount; i++)
            {
                if (combo.Items[i] is ComboBoxItem cbi && cbi.Tag?.ToString() == tag)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void DetailAddCommentBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var content = DetailNewComment.Text?.Trim();
            if (string.IsNullOrEmpty(content) || _selectedTicketId == Guid.Empty) return;

            _comments.Add(new InMemoryComment
            {
                TicketId = _selectedTicketId,
                Author = _currentUsername,
                Content = content,
                CreatedAt = DateTime.Now,
            });

            // DB stub: сохранение комментария в БД
            // TODO: раскомментировать после настройки БД
            // using (var db = new AppDbContext())
            // {
            //     db.Comments.Add(comment);
            //     db.SaveChanges();
            // }

            DetailNewComment.Text = "";

            // refresh detail view
            ShowTicketDetail(_selectedTicketId);
        }

        // ====================================================================
        //  HELPERS
        // ====================================================================

        private void ShowLoginError(string message)
        {
            LoginError.Text = message;
            LoginError.IsVisible = true;
        }

        private static string RoleDisplayName(UserRole role) => role switch
        {
            UserRole.Admin => "Администратор",
            UserRole.Agent => "Агент поддержки",
            _ => "Клиент",
        };

        private static string StatusDisplayName(TicketStatus status) => status switch
        {
            TicketStatus.New => "Новый",
            TicketStatus.InProgress => "В работе",
            TicketStatus.waitingForCustomer => "Ожидает ответа",
            TicketStatus.Resolved => "Решён",
            TicketStatus.Closed => "Закрыт",
            _ => "—",
        };

        private static string PriorityDisplayName(TicketPriority priority) => priority switch
        {
            TicketPriority.Low => "Низкий",
            TicketPriority.Medium => "Средний",
            TicketPriority.High => "Высокий",
            TicketPriority.Urgent => "Критичный",
            _ => "—",
        };

        private static Color PriorityColor(TicketPriority priority) => priority switch
        {
            TicketPriority.Low => Color.Parse("#388E3C"),
            TicketPriority.Medium => Color.Parse("#F57C00"),
            TicketPriority.High => Color.Parse("#E65100"),
            TicketPriority.Urgent => Color.Parse("#D32F2F"),
            _ => Color.Parse("#888888"),
        };

        private TicketListItem MapToItem(InMemoryTicket t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Status = StatusDisplayName(t.Status),
            Priority = PriorityDisplayName(t.Priority),
            PriorityColor = new SolidColorBrush(PriorityColor(t.Priority)),
            CreatorName = t.CreatorName,
            CreatedAt = t.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
        };
    }

    // ========================================================================
    //  IN-MEMORY MODELS (stubs until DB is connected)
    // ========================================================================

    internal class InMemoryUser
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.User;
    }

    internal class InMemoryTicket
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public TicketStatus Status { get; set; } = TicketStatus.New;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatorName { get; set; } = "";
        public string? AssigneeName { get; set; }
    }

    internal class InMemoryComment
    {
        public Guid TicketId { get; set; }
        public string Author { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ========================================================================
    //  VIEW MODELS (for data binding in ListBoxes)
    // ========================================================================

    internal class TicketListItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public SolidColorBrush PriorityColor { get; set; } = new(Colors.Gray);
        public string CreatorName { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public RelayCommand? AssignCommand { get; set; }
    }

    internal class UserListItem
    {
        public string Initials { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public RelayCommand? EditCommand { get; set; }
    }

    internal class CommentListItem
    {
        public string Author { get; set; } = "";
        public string Content { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    internal class NavEntry
    {
        public string Icon { get; }
        public string Label { get; }
        public string Tag { get; }
        public UserRole MinRole { get; }

        public NavEntry(string iconLabel, string tag, UserRole minRole)
        {
            // split icon from label
            var parts = iconLabel.Split(' ', 2);
            Icon = parts[0];
            Label = parts.Length > 1 ? parts[1] : "";
            Tag = tag;
            MinRole = minRole;
        }
    }

    // ========================================================================
    //  SIMPLE RELAY COMMAND
    // ========================================================================

    internal class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }

    // ========================================================================
    //  DB CONTEXT STUB
    //  TODO: раскомментировать после установки PostgreSQL и настройки подключения
    // ========================================================================
    // using Microsoft.EntityFrameworkCore;
    //
    // public class AppDbContext : DbContext
    // {
    //     public DbSet<User> Users { get; set; }
    //     public DbSet<Tickets> Tickets { get; set; }
    //     public DbSet<Comment> Comments { get; set; }
    //     public DbSet<SLAPolicy> SLAPolicies { get; set; }
    //     public DbSet<TicketHistory> TicketHistories { get; set; }
    //     public DbSet<Attachment> Attachments { get; set; }
    //
    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     {
    //         optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=helpdesk;Username=user;Password=pass");
    //     }
    // }
}

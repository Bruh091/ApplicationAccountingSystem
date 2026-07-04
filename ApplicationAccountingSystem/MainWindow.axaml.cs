using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Domain.Designation;
using Microsoft.Extensions.DependencyInjection;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Domain.Model;
using ApplicationAccountingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ApplicationAccountingSystem
{
    public partial class MainWindow : Window
    {
        // ----- session state -----
        private UserRole _currentRole;
        private Guid _currentUserId;
        private string _currentUsername = "";
        private bool _isLightTheme = true;
        private string _lastPanel = "";

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
            SaveSLABtn.Click += SaveSLABtn_Click;
            App.ThemeChanged += OnThemeChanged;

            this.SizeChanged += OnMainWindowSizeChanged;
        }

        // ====================================================================
        //  LOGIN / REGISTER
        // ====================================================================

        private async void LoginBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var username = LoginUsername.Text?.Trim();
                var password = LoginPassword.Text?.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowLoginError("Заполните все поля");
                    return;
                }

                using var scope = App.Services.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var user = await authService.LoginAsync(new LoginDto
                {
                    Username = username,
                    Password = password
                });

                if (user == null)
                {
                    ShowLoginError("Неверное имя пользователя или пароль");
                    return;
                }

                LoginSuccess(new CurrentUserSession
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Username = user.Username,
                    Role = user.Role,

                });
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private async void RegisterBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = LoginUsername.Text?.Trim();
            var password = LoginPassword.Text?.Trim();
            var roleIndex = LoginRoleCombo.SelectedIndex;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Заполните все поля");
                return;
            }

            var selectedRole = roleIndex switch
            {
                0 => UserRole.Admin,
                1 => UserRole.Agent,
                _ => UserRole.User,
            };

            try
            {
                using var scope = App.Services.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var registeredUser = await authService.RegisterAsync(new RegisterUserDto
                {
                    Username = username,
                    Password = password,
                    Email = $"{username}@example.com",
                    FullName = username,
                    Role = selectedRole
                    
                });

                LoginSuccess(new CurrentUserSession
                {
                    Id = registeredUser.Id,
                    FullName = registeredUser.FullName,
                    Username = registeredUser.Username,
                    Role = registeredUser.Role
                });
            }
            catch (InvalidOperationException ex)
            {
                ShowLoginError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void LoginSuccess(CurrentUserSession user)
        {
            _currentRole = user.Role;
            _currentUsername = user.Username;
            _currentUserId = user.Id;

            LoginPanel.IsVisible = false;
            MainWorkspace.IsVisible = true;

            UserInfoBar.Text = $"{user.FullName} ({RoleDisplayName(user.Role)})";

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

        private async void ShowPanel(string tag)
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
                    await RefreshMyTickets();
                    break;
                case "Queue":
                    QueuePanel.IsVisible = true;
                    await RefreshQueue();
                    break;
                case "Users":
                    UsersPanel.IsVisible = true;
                    await RefreshUsers();
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
                    await LoadSLAPolicies();
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

        private async Task LoadSLAPolicies()
        {
            using var scope = App.Services.CreateScope();
            var slaRepository = scope.ServiceProvider.GetRequiredService<ISLARepository>();
            var configuration = LoadConfiguration();

            await LoadSLAPolicy(slaRepository, configuration, TicketPriority.Low, "Low", SLALowResponse, SLALowResolution);
            await LoadSLAPolicy(slaRepository, configuration, TicketPriority.Medium, "Medium", SLAMediumResponse, SLAMediumResolution);
            await LoadSLAPolicy(slaRepository, configuration, TicketPriority.High, "High", SLAHighResponse, SLAHighResolution);
            await LoadSLAPolicy(slaRepository, configuration, TicketPriority.Urgent, "Critical", SLACriticalResponse, SLACriticalResolution);
        }

        private static async Task LoadSLAPolicy(ISLARepository slaRepository, IConfiguration configuration, TicketPriority priority, string configPrefix, TextBox responseBox, TextBox resolutionBox)
        {
            var policy = await slaRepository.GetSLAPolicyByPriorityAsync(priority);

            if (policy == null)
            {
                var responseHours = configuration.GetValue<int>($"SLA:{configPrefix}ResponseHours");
                var resolutionHours = configuration.GetValue<int>($"SLA:{configPrefix}ResolutionHours");

                if (responseHours <= 0 || resolutionHours <= 0)
                {
                    return;
                }

                policy = await slaRepository.CreateSLAAsync(new SLAPolicy
                {
                    Id = Guid.NewGuid(),
                    Priority = priority,
                    ResponseTimeInHours = responseHours,
                    ResolutionTimeInHours = resolutionHours,
                    IsActive = true
                });
            }

            responseBox.Text = policy.ResponseTimeInHours.ToString();
            resolutionBox.Text = policy.ResolutionTimeInHours.ToString();
        }

        private static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }

        private async void SaveSLABtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (!TryReadSLAValues(SLALowResponse, SLALowResolution, out var lowResponse, out var lowResolution)) return;
                if (!TryReadSLAValues(SLAMediumResponse, SLAMediumResolution, out var mediumResponse, out var mediumResolution)) return;
                if (!TryReadSLAValues(SLAHighResponse, SLAHighResolution, out var highResponse, out var highResolution)) return;
                if (!TryReadSLAValues(SLACriticalResponse, SLACriticalResolution, out var urgentResponse, out var urgentResolution)) return;

                using var scope = App.Services.CreateScope();
                var slaRepository = scope.ServiceProvider.GetRequiredService<ISLARepository>();

                await SaveSLAPolicy(slaRepository, TicketPriority.Low, lowResponse, lowResolution);
                await SaveSLAPolicy(slaRepository, TicketPriority.Medium, mediumResponse, mediumResolution);
                await SaveSLAPolicy(slaRepository, TicketPriority.High, highResponse, highResolution);
                await SaveSLAPolicy(slaRepository, TicketPriority.Urgent, urgentResponse, urgentResolution);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private static bool TryReadSLAValues(TextBox responseBox, TextBox resolutionBox, out int responseHours, out int resolutionHours)
        {
            var hasResponse = int.TryParse(responseBox.Text?.Trim(), out responseHours);
            var hasResolution = int.TryParse(resolutionBox.Text?.Trim(), out resolutionHours);

            return hasResponse && hasResolution && responseHours > 0 && resolutionHours > 0;
        }

        private static async Task SaveSLAPolicy(ISLARepository slaRepository, TicketPriority priority, int responseHours, int resolutionHours)
        {
            var policy = await slaRepository.GetSLAPolicyByPriorityAsync(priority);

            if (policy == null)
            {
                await slaRepository.CreateSLAAsync(new SLAPolicy
                {
                    Id = Guid.NewGuid(),
                    Priority = priority,
                    ResponseTimeInHours = responseHours,
                    ResolutionTimeInHours = resolutionHours,
                    IsActive = true
                });
                return;
            }

            policy.ResponseTimeInHours = responseHours;
            policy.ResolutionTimeInHours = resolutionHours;
            policy.IsActive = true;
            await slaRepository.UpdateSLAAsync(policy);
        }

        // ====================================================================
        //  DASHBOARD
        // ====================================================================

        private async void RefreshDashboard()
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                var tickets = await ticketService.GetAllTicketsAsync();
                var ticketList = tickets.ToList();

                var openCount = ticketList.Count(t => t.Status == TicketStatus.New);
                var inProgressCount = ticketList.Count(t => t.Status == TicketStatus.InProgress);
                var waitCount = ticketList.Count(t => t.Status == TicketStatus.waitingForCustomer);
                var resolvedCount = ticketList.Count(t => t.Status == TicketStatus.Resolved);
                var closedCount = ticketList.Count(t => t.Status == TicketStatus.Closed);

                StatTotalTickets.Text = ticketList.Count.ToString();
                StatOpenTickets.Text = openCount.ToString();
                StatInProgressTickets.Text = inProgressCount.ToString();
                StatWaitingTickets.Text = waitCount.ToString();
                StatResolvedTickets.Text = resolvedCount.ToString();
                StatClosedTickets.Text = closedCount.ToString();

                StatAvgResolution.Text = "— (данные появятся после подключения БД)";
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        // ====================================================================
        //  CREATE TICKET
        // ====================================================================

        private async void CreateTicketBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
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

                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                await ticketService.CreateTicketAsync(new CreateTicketDto
                {
                    Title = title,
                    Description = description ?? "",
                    Priority = priority,
                    CreatedById = _currentUserId
                });

                TicketTitle.Text = "";
                TicketDescription.Text = "";
                CreateTicketError.IsVisible = false;

                RefreshDashboard();
                ShowPanel("MyTickets");
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        // ====================================================================
        //  MY TICKETS
        // ====================================================================

        private async Task RefreshMyTickets()
        {
            try
            {
                _myTicketItems.Clear();

                IEnumerable<TicketDto> tickets;

                if (_currentRole == UserRole.Admin)
                {
                    using var scope = App.Services.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    tickets = await ticketService.GetAllTicketsAsync();
                }
                else if (_currentRole == UserRole.Agent)
                {
                    using var scope = App.Services.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    var allTickets = await ticketService.GetAllTicketsAsync();
                    tickets = allTickets.Where(t => t.CreatedById == _currentUserId || t.AssignedToId == _currentUserId);
                }
                else
                {
                    using var scope = App.Services.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    tickets = await ticketService.GetTicketsByUserIdAsync(_currentUserId);
                }

                foreach (var t in tickets.OrderByDescending(t => t.CreatedAt))
                {
                    _myTicketItems.Add(MapToItem(t));
                }

                MyTicketsList.ItemsSource = _myTicketItems;
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
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

        private async Task RefreshQueue()
        {
            try
            {
                _queueItems.Clear();

                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                var tickets = await ticketService.GetAllTicketsAsync();
                var openTickets = tickets
                    .Where(t => (t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress) && t.AssignedToId == null)
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.CreatedAt);

                foreach (var t in openTickets)
                {
                    var viewItem = MapToItem(t);
                    viewItem.AssignCommand = new RelayCommand(async () => await AssignTicket(t.Id));
                    _queueItems.Add(viewItem);
                }

                QueueList.ItemsSource = _queueItems;
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void QueueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (QueueList.SelectedItem is TicketListItem item)
            {
                ShowTicketDetail(item.Id);
            }
        }

        private async Task AssignTicket(Guid ticketId)
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                var ticket = await ticketService.GetTicketByIdAsync(ticketId);
                if (ticket == null) return;

                if (_currentRole == UserRole.Agent && ticket.CreatedById == _currentUserId)
                {
                    return;
                }

                await ticketService.AssignTicketAsync(ticketId, _currentUserId);
                await RefreshQueue();
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        // ====================================================================
        //  USERS
        // ====================================================================

        private async Task RefreshUsers()
        {
            _allUserItems.Clear();

            using var scope = App.Services.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var users = await userService.GetAllUsersAsync();

            foreach (var u in users.OrderBy(u => u.FullName))
            {
                var item = new UserListItem
                {
                    Id = u.Id,
                    Initials = u.FullName.Length >= 2 ? u.FullName[..2].ToUpper() : u.FullName.ToUpper(),
                    FullName = u.FullName,
                    Role = RoleDisplayName(u.Role),
                    RoleValue = u.Role,
                    EditCommand = new RelayCommand(async () => await EditUserRole(u.Id))
                };
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

        private async Task EditUserRole(Guid userId)
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var user = await userService.GetUserByIdAsync(userId);
                if (user == null) return;

                var selectedRole = await ShowRoleEditor(user);
                if (selectedRole == null) return;

                await userService.UpdateUserRoleAsync(userId, selectedRole.Value);
                await RefreshUsers();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private Task<UserRole?> ShowRoleEditor(UserDto user)
        {
            var dialog = new Window
            {
                Title = "Редактировать пользователя",
                Width = 360,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var adminItem = new ComboBoxItem { Content = "Администратор", Tag = UserRole.Admin };
            var agentItem = new ComboBoxItem { Content = "Агент поддержки", Tag = UserRole.Agent };
            var userItem = new ComboBoxItem { Content = "Клиент", Tag = UserRole.User };

            var roleCombo = new ComboBox
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Items =
                {
                    adminItem,
                    agentItem,
                    userItem
                }
            };

            roleCombo.SelectedItem = user.Role switch
            {
                UserRole.Admin => adminItem,
                UserRole.Agent => agentItem,
                _ => userItem
            };

            var saveButton = new Button { Content = "Сохранить", Classes = { "primary" }, Width = 120 };
            var cancelButton = new Button { Content = "Отмена", Classes = { "secondary" }, Width = 120 };

            saveButton.Click += (_, _) =>
            {
                if (roleCombo.SelectedItem is ComboBoxItem item && item.Tag is UserRole role)
                {
                    dialog.Close(role);
                }
            };

            cancelButton.Click += (_, _) => dialog.Close(null);

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = user.FullName, FontSize = 18, FontWeight = FontWeight.SemiBold },
                    new TextBlock { Text = user.Username, Opacity = 0.6 },
                    roleCombo,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { cancelButton, saveButton }
                    }
                }
            };

            return dialog.ShowDialog<UserRole?>(this);
        }

        // ====================================================================
        //  TICKET DETAIL
        // ====================================================================

        private async void ShowTicketDetail(Guid ticketId)
        {
            try
            {
                _selectedTicketId = ticketId;

                DashboardPanel.IsVisible = false;
                CreateTicketPanel.IsVisible = false;
                MyTicketsPanel.IsVisible = false;
                QueuePanel.IsVisible = false;
                UsersPanel.IsVisible = false;
                SLAPanel.IsVisible = false;
                TicketDetailPanel.IsVisible = true;

                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                var ticket = await ticketService.GetTicketByIdAsync(ticketId);
                if (ticket == null) return;

                DetailTitle.Text = ticket.Title;
                DetailStatus.Text = StatusDisplayName(ticket.Status);
                DetailPriority.Text = PriorityDisplayName(ticket.Priority);
                DetailCreator.Text = ticket.CreatorName;
                DetailAssignee.Text = ticket.AssigneeName ?? "—";
                DetailDescription.Text = ticket.Description;

                SelectComboByTag(DetailStatusCombo, ticket.Status.ToString());
                SelectComboByTag(DetailPriorityCombo, ticket.Priority.ToString());

                if (_currentRole <= UserRole.Agent)
                {
                    DetailActionsPanel.IsVisible = true;
                    DetailStatusCombo.IsEnabled = true;
                    DetailApplyStatusBtn.IsVisible = true;
                    DetailPriorityCombo.IsEnabled = true;
                    DetailApplyPriorityBtn.IsVisible = true;
                }
                else
                {
                    bool isOwn = ticket.CreatedById == _currentUserId;
                    DetailActionsPanel.IsVisible = isOwn;
                    DetailStatusCombo.IsEnabled = false;
                    DetailApplyStatusBtn.IsVisible = false;
                    DetailPriorityCombo.IsEnabled = isOwn;
                    DetailApplyPriorityBtn.IsVisible = isOwn;
                }

                _detailCommentItems.Clear();
                var commentService = scope.ServiceProvider.GetRequiredService<ICommentService>();
                var ticketComments = await commentService.GetCommentsByTicketIdAsync(ticketId);
                foreach (var c in ticketComments)
                {
                    _detailCommentItems.Add(new CommentListItem
                    {
                        Author = c.AuthorName,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    });
                }
                DetailComments.ItemsSource = _detailCommentItems;
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void DetailBackBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ShowPanel(_lastPanel);
        }

        private async void DetailApplyStatusBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (_selectedTicketId == Guid.Empty) return;
                if (DetailStatusCombo.SelectedItem is not ComboBoxItem item) return;

                var newStatus = item.Tag?.ToString();
                if (newStatus == null) return;

                if (!Enum.TryParse<TicketStatus>(newStatus, out var parsed)) return;
                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                await ticketService.ChangeStatusAsync(_selectedTicketId, parsed);

                ShowTicketDetail(_selectedTicketId);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private async void DetailApplyPriorityBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (_selectedTicketId == Guid.Empty) return;
                if (DetailPriorityCombo.SelectedItem is not ComboBoxItem item) return;

                var newPriority = item.Tag?.ToString();
                if (newPriority == null) return;

                if (!Enum.TryParse<TicketPriority>(newPriority, out var parsed)) return;
                using var scope = App.Services.CreateScope();
                var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                await ticketService.ChangePriorityAsync(_selectedTicketId, parsed);

                ShowTicketDetail(_selectedTicketId);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
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

        private async void DetailAddCommentBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var content = DetailNewComment.Text?.Trim();
                if (string.IsNullOrEmpty(content) || _selectedTicketId == Guid.Empty) return;

                using var scope = App.Services.CreateScope();
                var commentService = scope.ServiceProvider.GetRequiredService<ICommentService>();
                await commentService.AddCommentAsync(new CreateCommentDto
                {
                    TicketId = _selectedTicketId,
                    UserId = _currentUserId,
                    Content = content,
                    IsInternal = false
                });

                DetailNewComment.Text = "";
                ShowTicketDetail(_selectedTicketId);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        // ====================================================================
        //  HELPERS
        // ====================================================================

        private void ShowLoginError(string message)
        {
            LoginError.Text = message;
            LoginError.IsVisible = true;
        }

        private void ShowDatabaseError(Exception ex)
        {
            var message = $"Ошибка базы данных: {ex.Message}";

            if (LoginPanel.IsVisible)
            {
                ShowLoginError(message);
                return;
            }

            UserInfoBar.Text = message;
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

        private TicketListItem MapToItem(TicketDto t) => new()
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

    internal class CurrentUserSession
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.User;
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
        public Guid Id { get; set; }
        public string Initials { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public UserRole RoleValue { get; set; }
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

}

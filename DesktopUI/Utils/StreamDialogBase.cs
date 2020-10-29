﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Stylet;

namespace Speckle.DesktopUI.Utils
{
  public abstract class StreamDialogBase : Conductor<IScreen>.Collection.OneActive
  {
    protected ConnectorBindings Bindings;

    private int _selectedSlide;

    public int SelectedSlide
    {
      get => _selectedSlide;
      set => SetAndNotify(ref _selectedSlide, value);
    }

    private Account _accountToSendFrom = AccountManager.GetDefaultAccount();

    public Account AccountToSendFrom
    {
      get => _accountToSendFrom;
      set => SetAndNotify(ref _accountToSendFrom, value);
    }

    private int _selectionCount;

    public int SelectionCount
    {
      get => _selectionCount;
      set => SetAndNotify(ref _selectionCount, value);
    }

    public string ActiveViewName
    {
      get => Bindings.GetActiveViewName();
    }

    public List<string> ActiveViewObjects
    {
      get => Bindings.GetObjectsInView();
    }

    public List<string> CurrentSelection
    {
      get => Bindings.GetSelectedObjects();
    }

    private BindableCollection<FilterTab> _filterTabs;

    public BindableCollection<FilterTab> FilterTabs
    {
      get => _filterTabs;
      set => SetAndNotify(ref _filterTabs, value);
    }

    private FilterTab _selectedFilterTab;

    public FilterTab SelectedFilterTab
    {
      get => _selectedFilterTab;
      set { SetAndNotify(ref _selectedFilterTab, value); }
    }

    public void AddToSelection()
    {
      var newIds = Bindings.GetSelectedObjects().Except(SelectedFilterTab.ListItems);
      SelectedFilterTab.ListItems.AddRange(newIds);
    }

    public void ClearSelection()
    {
      SelectedFilterTab.ListItems.Clear();
    }

    public void RemoveFilterItem(string name)
    {
      SelectedFilterTab.RemoveListItem(name);
    }

    #region Adding Collaborators

    private Timer userSearchTimer;

    private string _userQuery;

    public string UserQuery
    {
      get => _userQuery;
      set
      {
        SetAndNotify(ref _userQuery, value);
        userSearchTimer?.Stop();

        if ( value == "" )
        {
          SelectedUser = null;
          UserSearchResults.Clear();
        }

        if ( SelectedUser != null ) return;
        userSearchTimer = new Timer(500) {AutoReset = false, Enabled = true};
        userSearchTimer.Elapsed += userSearchTimer_Elapsed;
      }
    }

    private BindableCollection<User> _userSearchResults;

    public BindableCollection<User> UserSearchResults
    {
      get => _userSearchResults;
      set => SetAndNotify(ref _userSearchResults, value);
    }

    private User _selectedUser;

    public User SelectedUser
    {
      get => _selectedUser;
      set
      {
        SetAndNotify(ref _selectedUser, value);
        if ( SelectedUser == null )
          return;
        UserQuery = SelectedUser.name;
        AddCollabToCollection(SelectedUser);
      }
    }

    private void userSearchTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      SearchForUsers();
    }

    public async void SearchForUsers()
    {
      if ( UserQuery == null || UserQuery.Length <= 2 )
        return;

      try
      {
        var client = new Client(AccountToSendFrom);
        var users = await client.UserSearch(UserQuery);
        UserSearchResults = new BindableCollection<User>(users);
      }
      catch ( Exception e )
      {
        Debug.WriteLine(e);
      }
    }

    private StreamRole _role;

    public StreamRole Role
    {
      get => _role;
      set => SetAndNotify(ref _role, value);
    }

    public BindableCollection<StreamRole> Roles { get; internal set; }

    private BindableCollection<User> _collaborators = new BindableCollection<User>();

    public BindableCollection<User> Collaborators
    {
      get => _collaborators;
      set => SetAndNotify(ref _collaborators, value);
    }

    private void AddCollabToCollection(User user)
    {
      if ( Collaborators.All(c => c.id != user.id) )
        Collaborators.Add(user);
    }

    public void RemoveCollabFromCollection(User user)
    {
      Collaborators.Remove(user);
    }

    #endregion

    public void CloseDialog()
    {
      DialogHost.CloseDialogCommand.Execute(null, null);
    }
  }

  public class StreamRole
  {
    public StreamRole(string name,  string description)
    {
      Name = name;
      Role = $"stream:{name}";
      Description = description;
    }

    public string Name { get; }
    public string Role { get; }
    public string Description { get; }
  }
}

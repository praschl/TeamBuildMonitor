﻿using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class TextWithLinkMessage : NotificationContent
    {
        private readonly Action<NotificationBase> _linkClickAction;

        public TextWithLinkMessage(string title, string message, string link, Action<NotificationBase> linkClickAction)
            : base(title)
        {
            Message = message;
            LinkText = link;
            _linkClickAction = linkClickAction;
        }

        public string Message { get; set; }

        public string LinkText { get; set; }

        public ICommand LinkClickCommand
        {
            get
            {
                return new LinkClickCommand(_linkClickAction, _notificationBase);
            }
        }
    }
}

﻿using System;

namespace VidaPolicial
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public readonly string Command;
        public readonly string HelpText;

        public CommandAttribute(string command, string helpText = "")
        {
            Command = command;
            HelpText = helpText;
        }

        public string Alias { get; set; } = string.Empty;
        public bool GreedyArg { get; set; } = false;
    }
}
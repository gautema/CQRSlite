using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSCode1_0.WriteModel.Commands
{
    public class TestCommand : ICommand
    {
        public TestCommand(string value)
        {
            Value = value;
        }

        public int ExpectedVersion { get; set; }

        public string Value { get; set; }
    }
}

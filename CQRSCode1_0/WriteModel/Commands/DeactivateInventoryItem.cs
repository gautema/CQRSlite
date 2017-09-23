﻿using System;
using CQRSlite.Commands;

namespace CQRSCode1_0.WriteModel.Commands
{
    public class DeactivateInventoryItem : ICommand 
	{
        public DeactivateInventoryItem(Guid id, int originalVersion)
        {
            Id = id;
            ExpectedVersion = originalVersion;
        }

        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}
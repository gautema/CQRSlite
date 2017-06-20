﻿using System;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public interface IRepository
    {
        Task Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot;
        Task<T> Get<T>(IIdentity aggregateId) where T : AggregateRoot;
    }
}
using System;
using EasyEventSourcing.EventSourcing.Domain;

namespace EasyEventSourcing.EventSourcing.Persistence
{
    public interface IRepository {
        T GetById<T>(Guid id) where T : EventStream;

        void Save(params EventStream[] streamItems);
    }
}
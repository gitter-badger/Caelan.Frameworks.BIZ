﻿namespace Caelan.Frameworks.BIZ.Classes

open Caelan.Frameworks.BIZ.Interfaces

module Extenders =
    type Repository with
        static member Entity<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager : IUnitOfWork) = 
            Repository<'TEntity>(manager)

    type Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> with
        member this.DTO<'TDTO when 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
            Repository<'TEntity, 'TDTO>(this.UnitOfWork)
﻿using AutoMapper;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
	public static class GenericBusinessBuilder
	{
		public static BaseDTOBuilder<TSource, TDestination> GenericDTOBuilder<TSource, TDestination>()
			where TSource : class, IEntity, new()
			where TDestination : class, IDTO, new()
		{
			var builder = new BaseDTOBuilder<TSource, TDestination>();

			if (Mapper.FindTypeMapFor<TSource, TDestination>() == null) Mapper.AddProfile(builder);

			return builder;
		}

		public static BaseEntityBuilder<TSource, TDestination> GenericEntityBuilder<TSource, TDestination>()
			where TSource : class, IDTO, new()
			where TDestination : class, IEntity, new()
		{
			var builder = new BaseEntityBuilder<TSource, TDestination>();

			if (Mapper.FindTypeMapFor<TSource, TDestination>() == null) Mapper.AddProfile(builder);

			return builder;
		}
	}
}

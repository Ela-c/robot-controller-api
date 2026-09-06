using FastMember;
using Npgsql;

namespace robot_controller_api.Persistence

{
	public static class ExtensionMethods
	{
		//allows us to trasnfer data from database to an object in the application
		public static void MapTo<T>(this NpgsqlDataReader dr, T entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			
			var fastMember = TypeAccessor.Create(entity.GetType());
			//hashset ensures all the property names are unique
			var props = fastMember.GetMembers().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            //loops throguh all columns from sql reader
            for (int i = 0; i < dr.FieldCount; i++)
			{
                string columnName = dr.GetName(i);
				// find matching property
				var prop = props.FirstOrDefault(x => x.Equals(columnName, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrEmpty(prop))
				{
                    // map column value to property of entity object
                    fastMember[entity, prop] = dr.IsDBNull(i) ? null : dr.GetValue(i);
				}
			}
		}
	}

}
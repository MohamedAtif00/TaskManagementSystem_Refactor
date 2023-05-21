using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Data
{
	public class DbInitializer
	{
		private readonly ModelBuilder modelBuilder;

		public DbInitializer(ModelBuilder modelBuilder)
		{
			this.modelBuilder = modelBuilder;
		}

		public void Seed()
		{
			Console.WriteLine("Seeding DB");
			// LEARNING OBJECTIVE TYPES
			var Interactive = new Schema
			{
				Id = 1,
				Name = "Interactive",
				Description = "An interactive application with learning objective.",
				Archived = false
			};
			var LiveVideo = new Schema
			{
				Id = 2,
				Name = "Live Video",
				Description = "A Live Video that explains the learning objective.",
				Archived = false
			};
			var Game = new Schema
			{
				Id = 3,
				Name = "Game",
				Description = "A gamified learning objective.",
				Archived = false
			};
			modelBuilder.Entity<Schema>().HasData(
				Interactive, LiveVideo, Game
			);

			// GROUPS
			var ID = new Group
			{
				Id = 1,
				Archived = false,
				Name = "ID",
			};
			var SME = new Group
			{
				Id = 2,
				Archived = false,
				Name = "SME",
			};
			var PM = new Group
			{
				Id = 3,
				Archived = false,
				Name = "PM",
			};
			var PR = new Group
			{
				Id = 4,
				Archived = false,
				Name = "PR",
			};
			var VO = new Group
			{
				Id = 5,
				Archived = false,
				Name = "VO",
			};
			var GD = new Group
			{
				Id = 6,
				Archived = false,
				Name = "GD",
			};
			modelBuilder.Entity<Group>().HasData(
				ID, SME, PM, PR, VO, GD
			);
		}
	}
}
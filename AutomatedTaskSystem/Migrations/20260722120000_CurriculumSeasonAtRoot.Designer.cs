using AutomatedTaskSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20260722120000_CurriculumSeasonAtRoot")]
    partial class CurriculumSeasonAtRoot
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.5");
#pragma warning restore 612, 618
        }
    }
}

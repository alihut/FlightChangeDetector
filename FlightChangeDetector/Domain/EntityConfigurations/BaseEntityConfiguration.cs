using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlightChangeDetector.Domain.EntityConfigurations
{
    public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual async void Configure(EntityTypeBuilder<T> builder)
        {
            // Configure common properties
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            //builder.HasQueryFilter(p => !p.IsDeleted && p.IsActive);
        }


    }

}

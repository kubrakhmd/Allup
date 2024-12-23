using System.Drawing;
using Allup.DAL;
using Microsoft.EntityFrameworkCore;

namespace Allup.Models
{
	public abstract  class BaseEntity
	{
		public int Id { get; set; }
		public bool IsDeleted { get; set; }	
		public DateTime CreatedAt { get; set; }
	}
}

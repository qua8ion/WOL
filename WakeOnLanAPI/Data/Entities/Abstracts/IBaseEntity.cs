using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbData.Entities.Abstracts
{
    public interface IBaseEntity
    {
        long Id { get; set; }

        long LastUpdateTick { get; set; }

        bool IsDeleted { get; set; }
    }
}

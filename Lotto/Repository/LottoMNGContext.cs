using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Lotto.Models;

namespace Lotto.Repository
{
    public class LottoMNGContext : DbContext
    {

        public LottoMNGContext() : base("GetRichEntities")
        {
        }

        public DbSet<Lotto_History> Lotto_Histories { get; set; }

    }
}
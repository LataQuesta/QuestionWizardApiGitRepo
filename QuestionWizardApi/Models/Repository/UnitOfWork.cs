using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionWizardApi.Models.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuestionDemoEntities db;
        public UnitOfWork(QuestionDemoEntities context)
        {
            db = context;
        }
        public int SaveChanges()
        {
            return db.SaveChanges();
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
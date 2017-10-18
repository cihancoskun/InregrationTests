using System;
using System.Collections.Generic;
using System.Text;

namespace InregrationTests.Entities
{
    public class User
    {
        public virtual Guid Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
    }
}

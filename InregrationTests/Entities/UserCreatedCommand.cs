using System;
using System.Collections.Generic;
using System.Text;

namespace InregrationTests.Entities
{
    public class UserCreatedCommand
    {
        public Guid CommandId { get; set; }
        public User Value { get; set; }
    }
}

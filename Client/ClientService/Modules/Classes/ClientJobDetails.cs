﻿using ClientService.Modules.Interfaces;

namespace ClientService.Modules.Classes
{
    public class ClientJobDetails : IClientJobDetails
    {
        public string JobNumber { get; set; } = string.Empty;
    }
}

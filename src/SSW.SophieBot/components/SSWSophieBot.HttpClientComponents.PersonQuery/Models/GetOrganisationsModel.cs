﻿using System.Collections.Generic;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class GetOrganisationsModel
    {
        public string OrganisationId { get; set; }
        public string Name { get; set; }
        public List<Location> Sites { get; set; }
        public List<UnifiControllerModel> Controllers { get; set; }
    }
}

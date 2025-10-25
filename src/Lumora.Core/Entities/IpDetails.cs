﻿using Lumora.Geography;

namespace Lumora.Entities;

[Table("ip_details")]
[Index(nameof(Ip), IsUnique = true)]
public class IpDetails
{
    [Key]
    public string Ip { get; set; } = string.Empty;

    public Continent ContinentCode { get; set; } = Continent.ZZ;

    public Country CountryCode { get; set; } = Country.ZZ;

    public string CityName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

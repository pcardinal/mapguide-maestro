#region Disclaimer / License

// Copyright (C) 2014, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using ProjNet.CoordinateSystems;
using System;

namespace MpuCalc
{
    internal class Program
    {
        // Meters per degree for geographic coordinate systems.
        // This matches the constant used in OSGeo.MapGuide.MaestroAPI's DegreeBasedCoordinateSystem:
        //   10_000_000 / 90  ≈  111_111.11 m/degree
        private const double DegreesToMeters = 10_000_000.0 / 90.0;

        private static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                try
                {
                    var csFact = new CoordinateSystemFactory();
                    var cs = csFact.CreateFromWkt(args[0]);

                    double mpu;
                    var unit = cs.GetUnits(0);

                    if (unit is AngularUnit)
                    {
                        // Geographic (lat/lon) coordinate system — unit is degrees
                        mpu = DegreesToMeters;
                    }
                    else if (unit is LinearUnit lu)
                    {
                        // Projected coordinate system — unit carries the m/unit conversion factor
                        mpu = lu.MetersPerUnit;
                    }
                    else
                    {
                        Console.WriteLine("Error: Unsupported coordinate system unit type");
                        return;
                    }

                    Console.WriteLine(mpu);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Error: Insufficient arguments. Usage: MpuCalc.exe [Coord sys WKT]");
            }
        }
    }
}
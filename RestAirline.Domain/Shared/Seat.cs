﻿namespace RestAirline.Domain.Shared
{
    public class Seat
    {
        public string Number { get; set; }

        public SeatType SeatType { get; set; }
    }

    public enum SeatType
    {
        Business,
        Economy
    }
}
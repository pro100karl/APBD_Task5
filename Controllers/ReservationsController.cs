using Microsoft.AspNetCore.Mvc;
using APBD_Task5.Models;
using System.Runtime.CompilerServices;

namespace APBD_Task5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<Reservation>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] string? status,
            [FromQuery] int? roomId
            )
        {
            var reservations = Database.DataStore.Reservations.AsEnumerable();
            if (date.HasValue)
                reservations = reservations.Where(r => r.Date == date.Value);
            if (!string.IsNullOrEmpty(status))
                reservations = reservations.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (roomId.HasValue)
                reservations = reservations.Where(r => r.RoomId == roomId.Value);

            return Ok(reservations.ToList());
        }
        [HttpGet("{id:int}")]
        public ActionResult<Reservation> GetById(int id)
        {
            var reservation = Database.DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return NotFound($"Reservation with Id {id} not found.");
            }
            return Ok(reservation);
        }
        [HttpPost]
        public ActionResult<Reservation> CreateReservation(Reservation reservation)
        {
            var room = Database.DataStore.Rooms.FirstOrDefault(r => r.Id == reservation.RoomId);
            if (room == null)

                return BadRequest($"Room with Id {reservation.RoomId} not found.");

            if (room.isActive == false)

                return BadRequest($"Room with Id {reservation.RoomId} is not active.");

            if (reservation.StartTime >= reservation.EndTime)

                return BadRequest("Start time must be before end time.");

            if (reservation.Date < DateOnly.FromDateTime(DateTime.Now))

                return BadRequest("Reservation date cannot be in the past.");

            var overlappingReservation = Database.DataStore.Reservations.FirstOrDefault(r =>
                 r.RoomId == reservation.RoomId &&
                 r.Date == reservation.Date &&
                 ((reservation.StartTime >= r.StartTime && reservation.StartTime < r.EndTime) || (reservation.EndTime > r.StartTime && reservation.EndTime <= r.EndTime) || (reservation.StartTime <= r.StartTime && reservation.EndTime >= r.EndTime)));
            
            if (overlappingReservation != null)

                return Conflict($"Room with Id {reservation.RoomId} is already reserved for the specified time slot.");

            reservation.Id = Database.DataStore.NextReservationId;
            Database.DataStore.Reservations.Add(reservation);
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }
        [HttpPut("{id:int}")]
        public ActionResult<Reservation> UpdateReservation(int id, Reservation updatedReservation)
        {
            var reservation = Database.DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            
               return NotFound($"Reservation with Id {id} not found.");

            var room = Database.DataStore.Rooms.FirstOrDefault(r => r.Id == updatedReservation.RoomId);
            if (room == null)
                return BadRequest($"Room with Id {updatedReservation.RoomId} not found.");

            if (room.isActive == false)
                return BadRequest($"Room with Id {updatedReservation.RoomId} is not active.");

            if (updatedReservation.StartTime >= updatedReservation.EndTime)
                return BadRequest($"Start time must be before end time.");

            if (updatedReservation.Date < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest($"Reservation date cannot be in the past.");

            var overlappingReservation = Database.DataStore.Reservations.FirstOrDefault(r =>
                    r.Id != id &&
                    r.RoomId == updatedReservation.RoomId &&
                    r.Date == updatedReservation.Date &&
                    ((updatedReservation.StartTime >= r.StartTime && updatedReservation.StartTime < r.EndTime) 
                    || (updatedReservation.EndTime > r.StartTime && updatedReservation.EndTime <= r.EndTime) 
                    || (updatedReservation.StartTime <= r.StartTime && updatedReservation.EndTime >= r.EndTime)));

            if (overlappingReservation != null)
                return Conflict($"Room with Id {updatedReservation.RoomId} is already reserved for the specified time slot.");

            reservation.RoomId = updatedReservation.RoomId;
            reservation.OrganizerName = updatedReservation.OrganizerName;
            reservation.Topic = updatedReservation.Topic;
            reservation.Date = updatedReservation.Date;
            reservation.StartTime = updatedReservation.StartTime;
            reservation.EndTime = updatedReservation.EndTime;
            return Ok(reservation);

        }
        [HttpDelete("{id:int}")]
        public ActionResult DeleteReservation(int id)
        {
            {
                var reservation = Database.DataStore.Reservations.FirstOrDefault(r => r.Id == id);
                if (reservation == null)
                {
                    return NotFound($"Reservation with Id {id} not found.");
                }
                Database.DataStore.Reservations.Remove(reservation);
                return NoContent();
            }
        }
    }
}

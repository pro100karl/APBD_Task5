using Microsoft.AspNetCore.Mvc;
using APBD_Task5.Models;

namespace APBD_Task5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<Room>> GetAll(
            [FromQuery] int? minCapacity,
            [FromQuery] bool? hasProjector,
            [FromQuery] bool activeOnly = false
            )
        {
            var rooms = Database.DataStore.Rooms.AsEnumerable();

            if (minCapacity.HasValue)
                rooms = rooms.Where(r => r.Capacity >= minCapacity.Value);
            if (activeOnly)
                rooms = rooms.Where(r => r.isActive);
            if (hasProjector.HasValue)
                rooms = rooms.Where(r => r.HasProjector == hasProjector.Value);
            {
                
            }

            return Ok(rooms.ToList());
        }
        [HttpGet("{id:int}")]
        public ActionResult<Room> GetById(int id)
        {
            var room = Database.DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound($"Room with Id {id} not found.");
            }
            return Ok(room);
        }
        [HttpPost]
        public ActionResult<Room> CreateRoom(Room room)
        {
            room.Id = Database.DataStore.NextRoomId;
            Database.DataStore.Rooms.Add(room);

            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);

        }
        [HttpGet("building/{buildingCode}")]
        public ActionResult<List<Room>> GetByBuilding(string buildingCode)
        {
            var rooms = Database.DataStore.Rooms.Where(r => r.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase)).ToList();
            if (rooms.Count == 0)
            {
                return NotFound($"No rooms found for building code {buildingCode}.");
            }
            return Ok(rooms);
        }
        [HttpPut("{id:int}")]
        public ActionResult<Room> UpdateRoom(int id, Room updatedRoom)
        {
            var room = Database.DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound($"Room with Id {id} not found.");
            }
            room.Name = updatedRoom.Name;
            room.BuildingCode = updatedRoom.BuildingCode;
            room.Floor = updatedRoom.Floor;
            room.Capacity = updatedRoom.Capacity;
            room.HasProjector = updatedRoom.HasProjector;
            room.isActive = updatedRoom.isActive;
            return Ok(room);
        }
        [HttpDelete("{id:int}")]
        public ActionResult DeleteRoom(int id)
        {
            var room = Database.DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound($"Room with Id {id} not found.");
            }
            Database.DataStore.Rooms.Remove(room);
            return NoContent();
        }
    }
}

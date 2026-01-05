using Audio;
using UnityEngine;

namespace Commands
{
    public class MoveCommand: Command
    {
        private Movable _movable;
        private Direction _direction;
        private Vector3 _directionVector;
        private float _distance;
        
        public MoveCommand(Movable movable, Direction direction, float distance)
        {
            _movable = movable;
            _direction = direction;
            _directionVector = DirectionToVector(direction);
            _distance = distance;
        }

        protected override void ExecuteCommand()
        {
            // 🧱 Ngăn lỗi nếu object đã bị destroy
            if (_movable == null || _movable.Equals(null)) return;

            if (_movable.CanMove(_directionVector, _distance))
            {
                _movable.Move(_directionVector, _distance);
                AudioManager.Instance?.PlayPlayerMoveSfx();
            }
            else
            {
                // kiểm tra vật cản
                GameObject obstacle = _movable.GetObstacle(_directionVector, _distance);
                if (obstacle != null
                    && obstacle.TryGetComponent(out Movable movableObstacle)
                    && !ReferenceEquals(_movable, movableObstacle)
                    && movableObstacle.CanMove(_directionVector, _distance))
                {
                    new MoveCommand(movableObstacle, _direction, _distance).Execute();
                    if (_movable == null || _movable.Equals(null)) return;
                    _movable.Move(_directionVector, _distance, force: true);
                    AudioManager.Instance?.PlayCrateMoveSfx();
                }
            }
        }

        public override void Undo()
        {
            if (_movable == null || _movable.Equals(null)) return;

            Direction opposite = GetOppositeDirection(_direction);
            Vector3 oppositeVec = DirectionToVector(opposite);
            _movable.Move(oppositeVec, _distance);
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.undoSfx);
        }


        public override void Redo()
        {
            ExecuteCommand();
        }
        
        public override Command Clone()
        {
            return new MoveCommand(_movable, _direction, _distance);
        }
        
        private Direction GetOppositeDirection(Direction direction)
        {
            return direction switch {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None
            };
        }
        
        private static Vector3 DirectionToVector(Direction direction)
        {
            return direction switch {
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                _ => Vector3.zero
            };
        }
    }
    
    public enum Direction { Up, Down, Left, Right, None }
}
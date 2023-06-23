using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBF.Math.Geometry.Curves {
	public class LinearCurve : ICurve {
		Bounds2D m_bounds;
		public Bounds2D Bounds {
			get { return m_bounds; }
		}

		public CurveSamplePoint Start { get; private set; }
		public CurveSamplePoint End { get; private set; }
		public float Length { get; private set; }

		List<Vector2> m_points;
		List<float> m_partialDistances;

		public LinearCurve() {
			m_partialDistances = new List<float>();
			m_bounds = new Bounds2D();

			SetPoints( new List<Vector2>() );
		}

		public LinearCurve( IEnumerable<Vector2> points ) {
			SetPoints( points );
		}

		void SetPoints( IEnumerable<Vector2> points ) {
			m_points = new List<Vector2>( points );
			m_partialDistances = new List<float>();

			m_bounds = new Bounds2D();
			foreach( Vector2 point in m_points )
				m_bounds.Encapsulate( point );

			if (m_points.Count == 0) {
				Start = new CurveSamplePoint() {
					Position = Vector2.zero, Direction = Vector2.right, Distance = 0, Time = 0
				};
				End = new CurveSamplePoint() {
					Position = Vector2.zero, Direction = Vector2.right, Distance = 0, Time = 1
				};
				Length = 0;
				return;
			}

			if (m_points.Count == 1) {
				Start = new CurveSamplePoint() {
					Position = m_points[0], Direction = Vector2.right, Distance = 0, Time = 0
				};
				End = new CurveSamplePoint() {
					Position = m_points[0], Direction = Vector2.right, Distance = 0, Time = 1
				};
				Length = 0;
				m_partialDistances.Add( 0 );
				return;
			}

			Vector2 dirStart = (m_points[1] - m_points[0]).normalized;
			Start = new CurveSamplePoint() {
				Position = m_points[0], Direction = dirStart, Distance = 0, Time = 0
			};

			Vector2 dirEnd = (m_points[m_points.Count - 1] - m_points[m_points.Count - 1 - 1]).normalized;
			End = new CurveSamplePoint() {
				Position = m_points.Last(), Direction = dirEnd, Distance = 0, Time = 1
			};

			Length = 0;
			m_partialDistances.Add( 0 );
			for (int i = 1; i < m_points.Count; i++) {
				Length += Vector2.Distance( m_points[i], m_points[i - 1] );
				m_partialDistances.Add( Length );
			}
		}

		public CurveSamplePoint GetPointAtDistance( float distance ) {
			if (distance <= 0) return Start;
			if (distance > Length) return End;

			for (int i = 1; i < m_points.Count; i++) {
				if (m_partialDistances[i] >= distance) {
					float k = Math.Map( m_partialDistances[i - 1], m_partialDistances[i], 0, 1, distance );
					return new CurveSamplePoint() {
						Position = Vector2.Lerp( m_points[i - 1], m_points[i], k ), Direction = (m_points[i] - m_points[i - 1]).normalized, Distance = distance, Time = distance / Length,
					};
				}
			}

			return End;
		}

		public CurveSamplePoint GetPointAtTime( float time ) {
			return GetPointAtDistance( time * Length );
		}

		public Vector2 GetAtDistance( float distance ) {
			CurveSamplePoint point = GetPointAtDistance( distance );
			return point.Position;
		}

		public Vector2 GetAtTime( float time ) {
			CurveSamplePoint point = GetPointAtTime( time );
			return point.Position;
		}

		public int GetLastIndexBefore( float time ) {
			float distance = Length * time;
			for (int i = 0; i < m_partialDistances.Count - 1; i++) {
				if (m_partialDistances[i] <= distance && m_partialDistances[i + 1] > distance)
					return i;
			}
			return m_partialDistances.Count - 1;
		}

		public CurveSamplePoint GetClosestPoint( Vector2 from ) {
			CurveSamplePoint bestPoint = Start;
			float bestDistanceSq = float.MaxValue;
			for (int i = 1; i < m_points.Count; i++) {
				float minDistSq1 = Vector2.SqrMagnitude( m_points[i] - from );
				float minDistSq2 = Vector2.SqrMagnitude( m_points[i - 1] - from );
				float linLenSq = Vector2.SqrMagnitude( m_points[i] - m_points[i - 1] );

				Vector2 proj = Math.ProjectPointSegment( from, m_points[i], m_points[i - 1] );
				float distSq = Vector2.SqrMagnitude( proj - from );
				if (distSq > bestDistanceSq) continue;

				float k = Vector2.Distance( proj, m_points[i - 1] ) / Vector2.Distance( m_points[i], m_points[i - 1] );
				bestPoint = new CurveSamplePoint() {
					Position = Vector2.Lerp( m_points[i - 1], m_points[i], k ), Direction = (m_points[i] - m_points[i - 1]).normalized, Distance = Mathf.Lerp( m_partialDistances[i - 1], m_partialDistances[i], k ), Time = Mathf.Lerp( m_partialDistances[i - 1], m_partialDistances[i], k ) / Length,
				};
				bestDistanceSq = distSq;
			}

			return bestPoint;
		}

		public Vector2 GetClosestPosition( Vector2 from ) {
			Vector2 bestPoint = Vector2.zero;
			float bestDistanceSq = float.MaxValue;
			for (int i = 1; i < m_points.Count; i++) {
				float minDistSq1 = Vector2.SqrMagnitude( m_points[i] - from );
				float minDistSq2 = Vector2.SqrMagnitude( m_points[i - 1] - from );
				float linLenSq = Vector2.SqrMagnitude( m_points[i] - m_points[i - 1] );

				if (minDistSq1 - linLenSq > bestDistanceSq && minDistSq2 - linLenSq > bestDistanceSq) continue;

				Vector2 proj = Math.ProjectPointSegment( from, m_points[i], m_points[i - 1] );
				float distSq = Vector2.SqrMagnitude( proj - from );
				if (distSq < bestDistanceSq) {
					float k = Vector2.Distance( proj, m_points[i - 1] ) / Vector2.Distance( m_points[i], m_points[i - 1] );
					bestPoint = Vector2.Lerp( m_points[i - 1], m_points[i], k );
					bestDistanceSq = distSq;
				}
			}

			return bestPoint;
		}

		public int GetClosestIndex( Vector2 from ) {
			int bestIndex = -1;
			float bestDistanceSq = float.MaxValue;
			for (int i = 0; i < m_points.Count; i++) {
				float distSq = Vector2.SqrMagnitude( m_points[i] - from );
				if (distSq < bestDistanceSq) {
					bestIndex = i;
					bestDistanceSq = distSq;
				}
			}

			return bestIndex;
		}
	}
}
using UnityEngine;

public class SimpleMovement: EnemyAttribute
{
    public override void UpdateAttribute(float deltaTime)
    {
        Vector2 dir = (self.target.position - transform.position).normalized;
        self.movementDirection = dir;
        self.rb.MovePosition(self.rb.position + dir * self.speed * deltaTime);
    }
}

public class CircleMovement : EnemyAttribute
{
    public float circleRadius = 3f;
    public float approachSpeed = 0.5f;
    public float circleSpeed = 1f;

    private float angle;

    public override void UpdateAttribute(float deltaTime)
    {
        angle += circleSpeed * deltaTime;

        Vector2 circleOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circleRadius;

        Vector2 targetPosition = (Vector2)self.target.position + circleOffset;

        Vector2 dir = (targetPosition - self.rb.position).normalized;

        self.movementDirection = (self.target.position - transform.position).normalized;;

        self.rb.MovePosition(self.rb.position + dir * self.speed * deltaTime);
    }
}
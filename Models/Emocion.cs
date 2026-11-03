namespace Laika.Models;

public record Emocion(string Nombre, string Color, string Descripcion, string Consejo)
{
    public static readonly List<Emocion> Todas = new()
    {
        new("Alegría",   "#FFB347", "Te sientes bien, con energía.",           "Compártela: escribe a alguien algo bonito hoy. La alegría crece cuando se reparte."),
        new("Calma",     "#4ECDC4", "Estás tranquilo/a, en paz.",              "Aprovecha para descansar la mente. Respira profundo y nota qué te ayudó a llegar aquí."),
        new("Tristeza",  "#7B9CFF", "Algo te pesa o extrañas algo.",           "Está bien llorar. Háblalo con alguien de confianza; no tienes que cargarlo solo/a."),
        new("Miedo",     "#A58BFF", "Sientes peligro o incertidumbre.",        "Respira lento (4 segundos adentro, 6 afuera). Si el miedo viene de alguien, avisa a un adulto o usa la sección Reportar."),
        new("Enojo",     "#FF7A7A", "Algo te pareció injusto o te lastimó.",   "Aléjate un momento antes de responder. Camina o mueve el cuerpo, y luego di lo que sientes con calma."),
        new("Sorpresa",  "#F5A3C7", "Pasó algo que no esperabas.",             "Date unos segundos para entender qué sientes. Puede ser bueno o difícil, y ambas cosas son válidas.")
    };
}

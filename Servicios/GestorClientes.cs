namespace SistemaAsesoria2.Servicios;

public record ClienteRegistrado(
    string Usuario,
    string NombreApellido,
    string Email,
    int Edad,
    string Password
);

public class GestorClientes
{
    private readonly Dictionary<string, ClienteRegistrado> _clientes =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly object _lock = new();

    public bool Registrar(ClienteRegistrado cliente)
    {
        lock (_lock)
        {
            if (_clientes.ContainsKey(cliente.Usuario))
            {
                return false;
            }

            _clientes[cliente.Usuario] = cliente;
            return true;
        }
    }

    public ClienteRegistrado? ValidarCredenciales(string usuario, string password)
    {
        lock (_lock)
        {
            if (_clientes.TryGetValue(usuario, out var cliente) &&
                cliente.Password.Equals(password))
            {
                return cliente;
            }

            return null;
        }
    }

    public ClienteRegistrado? ObtenerPorUsuario(string usuario)
    {
        lock (_lock)
        {
            return _clientes.TryGetValue(usuario, out var cliente) ? cliente : null;
        }
    }
}

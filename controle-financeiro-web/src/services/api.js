// Tenta HTTPS (7184) por padrão; se estiver usando HTTP, ajuste para 'http://localhost:5188/api'
const API_BASE_URL = 'https://localhost:7184/api';

async function request(endpoint, options = {}) {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    let mensagemErro = `Erro ${response.status}`;
    try {
      const data = await response.json();
      mensagemErro = data.erro || data.title || mensagemErro;
    } catch {
      const texto = await response.text();
      if (texto) mensagemErro = texto;
    }
    throw new Error(mensagemErro);
  }

  if (response.status === 204) return null;
  return response.json();
}

export const api = {
  baseUrl: API_BASE_URL,
  listarBancos: (filtro = '') =>
    request(`/bancos${filtro ? `?filtro=${encodeURIComponent(filtro)}` : ''}`),
  listarContas: (incluirInativas = false) =>
    request(`/contas?incluirInativas=${incluirInativas}`),
  obterResumoDashboard: (dataCorte) =>
    request(`/contas/resumo-dashboard${dataCorte ? `?dataCorte=${dataCorte}` : ''}`),
  criarConta: (dados) =>
    request('/contas', {
      method: 'POST',
      body: JSON.stringify(dados),
    }),
  atualizarConta: (id, dados) =>
    request(`/contas/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dados),
    }),
  alterarStatusConta: (id, ativa) =>
    request(`/contas/${id}/status?ativa=${ativa}`, {
      method: 'PATCH',
    }),
  listarTransacoes: () => request('/Transacoes'),
};
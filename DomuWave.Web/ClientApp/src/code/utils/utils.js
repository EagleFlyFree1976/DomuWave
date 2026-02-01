
export function toLookupEntity(items) {
  // Esempio di elaborazione standard della risposta
  const output = items.map(item => ({
    id: item.id,
    code: item.name,
    description: item.description
    
  }));
  return output;
}


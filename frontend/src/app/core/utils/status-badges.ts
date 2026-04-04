export interface StatusBadge {
  label: string;
  classes: string;
}

export function getProjectStatusBadge(status: string): StatusBadge {
  switch (status) {
    case 'InProgress':
      return { label: 'In Progress', classes: 'bg-sky-50 text-sky-700 border border-sky-100' };
    case 'Completed':
      return { label: 'Completed', classes: 'bg-emerald-50 text-emerald-700 border border-emerald-100' };
    case 'Cancelled':
      return { label: 'Cancelled', classes: 'bg-rose-50 text-rose-700 border border-rose-100' };
    default:
      return { label: 'Open', classes: 'bg-indigo-50 text-indigo-700 border border-indigo-100' };
  }
}

export function getBidStatusBadge(status: string): StatusBadge {
  switch (status) {
    case 'Accepted':
      return { label: 'Accepted', classes: 'bg-emerald-50 text-emerald-700 border border-emerald-100' };
    case 'Rejected':
      return { label: 'Rejected', classes: 'bg-rose-50 text-rose-700 border border-rose-100' };
    default:
      return { label: 'Pending', classes: 'bg-amber-50 text-amber-700 border border-amber-100' };
  }
}

export function getProfileStatusBadge(responseTime: string): StatusBadge {
  const normalized = responseTime.toLowerCase();

  if (normalized.includes('1 hour') || normalized.includes('2 hours') || normalized.includes('same day')) {
    return { label: 'Available', classes: 'bg-emerald-50 text-emerald-700 border border-emerald-100' };
  }

  return { label: 'Busy', classes: 'bg-orange-50 text-orange-700 border border-orange-100' };
}

export function getConversationStatusBadge(hasMessages: boolean): StatusBadge {
  return hasMessages
    ? { label: 'Active', classes: 'bg-sky-50 text-sky-700 border border-sky-100' }
    : { label: 'New', classes: 'bg-slate-100 text-slate-700 border border-slate-200' };
}

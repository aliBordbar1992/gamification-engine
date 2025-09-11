import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { walletApi } from '../api/wallet'
import type {
  SpendPointsRequestDto,
  TransferPointsRequestDto,
} from '../api/wallet'

// Query keys for wallet data
export const walletKeys = {
  all: ['wallets'] as const,
  userWallets: (userId: string) => [...walletKeys.all, 'user', userId] as const,
  wallet: (userId: string, pointCategoryId: string) =>
    [...walletKeys.all, 'wallet', userId, pointCategoryId] as const,
  balance: (userId: string, pointCategoryId: string) =>
    [...walletKeys.all, 'balance', userId, pointCategoryId] as const,
  transactions: (
    userId: string,
    pointCategoryId: string,
    from?: string,
    to?: string
  ) =>
    [
      ...walletKeys.all,
      'transactions',
      userId,
      pointCategoryId,
      from,
      to,
    ] as const,
}

/**
 * Hook to get all wallets for a user
 */
export const useUserWallets = (userId: string) => {
  return useQuery({
    queryKey: walletKeys.userWallets(userId),
    queryFn: () => walletApi.getUserWallets(userId),
    enabled: !!userId,
  })
}

/**
 * Hook to get a specific wallet
 */
export const useWallet = (userId: string, pointCategoryId: string) => {
  return useQuery({
    queryKey: walletKeys.wallet(userId, pointCategoryId),
    queryFn: () => walletApi.getWallet(userId, pointCategoryId),
    enabled: !!userId && !!pointCategoryId,
  })
}

/**
 * Hook to get wallet balance
 */
export const useWalletBalance = (userId: string, pointCategoryId: string) => {
  return useQuery({
    queryKey: walletKeys.balance(userId, pointCategoryId),
    queryFn: () => walletApi.getWalletBalance(userId, pointCategoryId),
    enabled: !!userId && !!pointCategoryId,
  })
}

/**
 * Hook to get wallet transaction history
 */
export const useWalletTransactions = (
  userId: string,
  pointCategoryId: string,
  from?: string,
  to?: string
) => {
  return useQuery({
    queryKey: walletKeys.transactions(userId, pointCategoryId, from, to),
    queryFn: () =>
      walletApi.getWalletTransactions(userId, pointCategoryId, from, to),
    enabled: !!userId && !!pointCategoryId,
  })
}

/**
 * Hook to spend points from a wallet
 */
export const useSpendPoints = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      userId,
      request,
    }: {
      userId: string
      request: SpendPointsRequestDto
    }) => walletApi.spendPoints(userId, request),
    onSuccess: (data, variables) => {
      // Invalidate related queries
      queryClient.invalidateQueries({
        queryKey: walletKeys.userWallets(variables.userId),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.wallet(
          variables.userId,
          variables.request.pointCategoryId!
        ),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.balance(
          variables.userId,
          variables.request.pointCategoryId!
        ),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.transactions(
          variables.userId,
          variables.request.pointCategoryId!
        ),
      })
    },
  })
}

/**
 * Hook to transfer points between users
 */
export const useTransferPoints = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      fromUserId,
      request,
    }: {
      fromUserId: string
      request: TransferPointsRequestDto
    }) => walletApi.transferPoints(fromUserId, request),
    onSuccess: (data, variables) => {
      // Invalidate queries for both users
      queryClient.invalidateQueries({
        queryKey: walletKeys.userWallets(variables.fromUserId),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.wallet(
          variables.fromUserId,
          variables.request.pointCategoryId!
        ),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.balance(
          variables.fromUserId,
          variables.request.pointCategoryId!
        ),
      })
      queryClient.invalidateQueries({
        queryKey: walletKeys.transactions(
          variables.fromUserId,
          variables.request.pointCategoryId!
        ),
      })

      // Also invalidate for the recipient
      if (variables.request.toUserId) {
        queryClient.invalidateQueries({
          queryKey: walletKeys.userWallets(variables.request.toUserId),
        })
        queryClient.invalidateQueries({
          queryKey: walletKeys.wallet(
            variables.request.toUserId,
            variables.request.pointCategoryId!
          ),
        })
        queryClient.invalidateQueries({
          queryKey: walletKeys.balance(
            variables.request.toUserId,
            variables.request.pointCategoryId!
          ),
        })
        queryClient.invalidateQueries({
          queryKey: walletKeys.transactions(
            variables.request.toUserId,
            variables.request.pointCategoryId!
          ),
        })
      }
    },
  })
}

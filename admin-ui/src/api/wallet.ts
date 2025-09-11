import type {
  WalletDto,
  WalletTransactionDto,
  WalletTransferDto,
  SpendPointsRequestDto,
  TransferPointsRequestDto,
} from './generated/models'
import { WalletApiInstance } from './generated-client'

// Wallet API
export const walletApi = {
  // Get all wallets for a user
  getUserWallets: async (userId: string): Promise<WalletDto[]> => {
    const response = await WalletApiInstance().apiWalletUsersUserIdGet(userId)
    return response.data
  },

  // Get a specific wallet for a user and point category
  getWallet: async (
    userId: string,
    pointCategoryId: string
  ): Promise<WalletDto> => {
    const response =
      await WalletApiInstance().apiWalletUsersUserIdCategoriesPointCategoryIdGet(
        userId,
        pointCategoryId
      )
    return response.data
  },

  // Get wallet balance for a user and point category
  getWalletBalance: async (
    userId: string,
    pointCategoryId: string
  ): Promise<number> => {
    const response =
      await WalletApiInstance().apiWalletUsersUserIdCategoriesPointCategoryIdBalanceGet(
        userId,
        pointCategoryId
      )
    return response.data
  },

  // Get transaction history for a wallet
  getWalletTransactions: async (
    userId: string,
    pointCategoryId: string,
    from?: string,
    to?: string
  ): Promise<WalletTransactionDto[]> => {
    const response =
      await WalletApiInstance().apiWalletUsersUserIdCategoriesPointCategoryIdTransactionsGet(
        userId,
        pointCategoryId,
        from,
        to
      )
    return response.data
  },

  // Spend points from a wallet
  spendPoints: async (
    userId: string,
    request: SpendPointsRequestDto
  ): Promise<WalletTransactionDto> => {
    const response = await WalletApiInstance().apiWalletUsersUserIdSpendPost(
      userId,
      request
    )
    return response.data
  },

  // Transfer points between users
  transferPoints: async (
    fromUserId: string,
    request: TransferPointsRequestDto
  ): Promise<WalletTransferDto> => {
    const response =
      await WalletApiInstance().apiWalletUsersFromUserIdTransferPost(
        fromUserId,
        request
      )
    return response.data
  },
}

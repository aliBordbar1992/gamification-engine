import React, { useState } from 'react'
import { Typography, Card, Alert, Spin, Pagination } from 'antd'
import { useLeaderboard, useRefreshLeaderboard, leaderboardsKeys } from '@/hooks/useLeaderboards'
import { useQueryClient } from '@tanstack/react-query'
import LeaderboardTable from '@/components/leaderboards/LeaderboardTable'
import LeaderboardChart from '@/components/leaderboards/LeaderboardChart'
import LeaderboardSelector from '@/components/leaderboards/LeaderboardSelector'
import type { LeaderboardType, TimeRange } from '@/api/leaderboards'

const { Title } = Typography

const Leaderboards: React.FC = () => {
  const [type, setType] = useState<LeaderboardType>('points')
  const [category, setCategory] = useState<string | undefined>(undefined)
  const [timeRange, setTimeRange] = useState<TimeRange>('alltime')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const queryClient = useQueryClient()
  const { data: leaderboard, isLoading, error } = useLeaderboard({
    type,
    category,
    timeRange,
    page,
    pageSize,
  })

  const refreshMutation = useRefreshLeaderboard()

  const handleTypeChange = (newType: LeaderboardType) => {
    setType(newType)
    setCategory(undefined) // Reset category when type changes
    setPage(1) // Reset to first page
    // Clear any cached leaderboard data to prevent stale data issues
    queryClient.removeQueries({ queryKey: leaderboardsKeys.all })
  }

  const handleCategoryChange = (newCategory: string | undefined) => {
    setCategory(newCategory)
    setPage(1) // Reset to first page
  }

  const handleTimeRangeChange = (newTimeRange: TimeRange) => {
    setTimeRange(newTimeRange)
    setPage(1) // Reset to first page
  }

  const handleRefresh = () => {
    refreshMutation.mutate({ type, category, timeRange })
  }

  const handlePageChange = (newPage: number) => {
    setPage(newPage)
  }

  const getTitle = () => {
    const typeLabels = {
      points: 'Points',
      badges: 'Badges',
      trophies: 'Trophies',
      level: 'Levels',
    }
    
    const timeLabels = {
      daily: 'Daily',
      weekly: 'Weekly',
      monthly: 'Monthly',
      alltime: 'All Time',
    }

    let title = `${typeLabels[type]} Leaderboard`
    if (category) {
      title += ` - ${category}`
    }
    title += ` (${timeLabels[timeRange]})`

    return title
  }

  return (
    <div>
      <Title level={2}>Leaderboards</Title>
      
      <LeaderboardSelector
        type={type}
        category={category}
        timeRange={timeRange}
        onTypeChange={handleTypeChange}
        onCategoryChange={handleCategoryChange}
        onTimeRangeChange={handleTimeRangeChange}
        onRefresh={handleRefresh}
        loading={refreshMutation.isPending}
      />

      {error && (
        <Alert
          message="Error loading leaderboard"
          description={error.message || 'An error occurred while loading the leaderboard.'}
          type="error"
          style={{ marginTop: '16px' }}
        />
      )}

      {!isLoading && !leaderboard && !error && (type === 'points' || type === 'level') && !category && (
        <Alert
          message="Category Required"
          description={`Please select a category to view the ${type} leaderboard.`}
          type="info"
          style={{ marginTop: '16px' }}
        />
      )}

      {leaderboard && (
        <>
          <Card style={{ marginTop: '16px' }}>
            <Title level={3}>{getTitle()}</Title>
            <LeaderboardChart data={leaderboard} type={type} />
          </Card>

          <Card style={{ marginTop: '16px' }}>
            <Spin spinning={isLoading}>
              <LeaderboardTable data={leaderboard} type={type} loading={isLoading} />
              
              {leaderboard.totalPages && leaderboard.totalPages > 1 && (
                <div style={{ textAlign: 'center', marginTop: '16px' }}>
                  <Pagination
                    current={page}
                    total={leaderboard.totalCount}
                    pageSize={pageSize}
                    onChange={handlePageChange}
                    showSizeChanger={false}
                    showQuickJumper
                    showTotal={(total, range) =>
                      `${range[0]}-${range[1]} of ${total} entries`
                    }
                  />
                </div>
              )}
            </Spin>
          </Card>
        </>
      )}
    </div>
  )
}

export default Leaderboards

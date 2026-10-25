#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "InputActionValue.h"
#include "StudentCharacter.generated.h"

class UCameraComponent;
class UInputAction;
class UInputMappingContext;

/**
 * Student 第一人称角色。
 * 只负责：移动、视角、奔跑、下蹲（Enhanced Input）。
 */
UCLASS()
class STUDENT_API AStudentCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	AStudentCharacter();

protected:
	virtual void BeginPlay() override;
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	/** 第一人称摄像机组件。 */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Camera")
	TObjectPtr<UCameraComponent> FirstPersonCameraComponent;

	/** Enhanced Input Mapping Context。 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
	TObjectPtr<UInputMappingContext> DefaultMappingContext;

	/** 移动输入动作（2D Axis: X/Y）。 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
	TObjectPtr<UInputAction> MoveAction;

	/** 视角输入动作（2D Axis: X/Y）。 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
	TObjectPtr<UInputAction> LookAction;

	/** 奔跑按下动作（Digital/Bool）。 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
	TObjectPtr<UInputAction> RunAction;

	/** 下蹲按下动作（Digital/Bool）。 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
	TObjectPtr<UInputAction> CrouchAction;

	/** 行走速度（可在编辑器调整）。 */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement", meta = (ClampMin = "0.0"))
	float WalkSpeed = 300.0f;

	/** 奔跑速度（可在编辑器调整）。 */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement", meta = (ClampMin = "0.0"))
	float RunSpeed = 600.0f;

	/** 下蹲移动速度（可在编辑器调整）。 */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement", meta = (ClampMin = "0.0"))
	float CrouchSpeed = 150.0f;

private:
	/** 处理 WASD 移动输入。 */
	void Move(const FInputActionValue& Value);

	/** 处理鼠标视角输入。 */
	void Look(const FInputActionValue& Value);

	/** 开始奔跑。 */
	void StartRun();

	/** 结束奔跑。 */
	void StopRun();

	/** 开始下蹲。 */
	void StartCrouch();

	/** 结束下蹲。 */
	void StopCrouch();

	/** 按当前状态刷新最大移动速度。 */
	void UpdateMaxWalkSpeed();

	/** 当前是否按住奔跑键。 */
	bool bIsRunning = false;
};
